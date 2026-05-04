import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables, ChartDataset } from 'chart.js';
import {
  Field,
  Dataset,
  ApiService,
  QueryRequest,
  Relationship,
} from '../../../services/api.service';
import { AnalyticsBuilderService } from '../../../services/analytics-builder.service';
Chart.register(...registerables);

export interface WidgetFilter {
  field: string;
  op: string;
  value: string;
}

export interface WidgetConfig {
  id: string;
  title: string;
  type:
    | 'bar'
    | 'line'
    | 'pie'
    | 'kpi'
    | 'grid'
    | 'hbar'
    | 'area'
    | 'stacked'
    | 'doughnut';
  dataset: string;
  dimensions: Field[];
  measures: Field[];
  aggregations: Record<string, string>;
  filters: WidgetFilter[];
  span: number;
  data?: any;
  loading?: boolean;
  error?: string;
  warning?: string;
  // Grid state
  sortCol?: string;
  sortDir?: 'asc' | 'desc';
  page: number;
  pageSize: number;
  // Data view toggle
  showDataView: boolean;
  // Cross-table join
  joinDataset?: string;
  // Query results panel
  showQueryResults: boolean;
}

const COLORS = [
  '#3b82f6',
  '#10b981',
  '#f59e0b',
  '#8b5cf6',
  '#06b6d4',
  '#ec4899',
  '#34d399',
  '#fbbf24',
  '#f87171',
  '#a78bfa',
];

const AGG_OPTIONS = ['SUM', 'AVG', 'COUNT', 'MIN', 'MAX'];
const FILTER_OPS = ['=', '!=', '>', '<', '>=', '<=', 'IN', 'LIKE'];

@Component({
  selector: 'app-builder',
  imports: [CommonModule, FormsModule],
  templateUrl: './builder.html',
  styleUrl: './builder.css',
})
export class Builder implements OnInit {
  datasets: Dataset[] = [];
  filteredDatasets: Dataset[] = [];
  widgets: WidgetConfig[] = [];
  selectedId: string | null = null;
  searchQuery = '';
  openDatasets = new Set<string>();
  draggingField: (Field & { ds: string }) | null = null;
  runningAll = false;
  toast = '';

  // Save/Load state
  dashboardName = '';
  currentDashboardId: string | null = null;
  savedDashboards: any[] = [];
  showSaveDialog = false;
  showLoadDialog = false;

  // Collapsible panels
  showFieldPanel = true;
  showConfigPanel = true;

  // Help popup
  showHelp = false;

  // Relationships (for cross-table joins)
  relationships: Relationship[] = [];

  // Filter autocomplete cache
  filterValuesCache = new Map<string, string[]>();
  filterValuesLoading = false;

  // Expose to template
  aggOptions = AGG_OPTIONS;
  filterOps = FILTER_OPS;

  private charts = new Map<string, Chart>();
  private idCounter = 0;
  private autoRunTimers = new Map<string, any>();

  constructor(
    private api: ApiService,
    private builderService: AnalyticsBuilderService,
  ) {}

  ngOnInit() {
    this.api.getDatasets().subscribe({
      next: (d) => {
        this.datasets = d;
        this.filteredDatasets = d;
        if (d.length > 0) this.openDatasets.add(d[0].id);
        if (d.length > 1) this.openDatasets.add(d[1].id);
        console.log.apply(this.datasets);
        // Try loading a preset first, then auto-load demo from DB
        const hasPreset = this.loadPresetIfAny();
        if (!hasPreset) {
          this.autoLoadDemoDashboard();
        }
      },
    });
    // Load available relationships
    this.api.getRelationships().subscribe({
      next: (r) => (this.relationships = r),
    });
  }

  toggleFieldPanel() {
    this.showFieldPanel = !this.showFieldPanel;
  }
  toggleConfigPanel() {
    this.showConfigPanel = !this.showConfigPanel;
  }
  toggleHelp() {
    this.showHelp = !this.showHelp;
  }

  private autoLoadDemoDashboard() {
    this.builderService.getBuilderDashboardsFromDb().subscribe({
      next: (dashboards) => {
        if (dashboards.length > 0) {
          const demo = dashboards[0];
          this._loadDashboardData(demo);
        }
      },
    });
  }

  private _loadDashboardData(db: any) {
    this.clearCanvas();
    this.dashboardName = db.name;
    this.currentDashboardId = db.id;
    for (const wCfg of db.widgets || []) {
      const wid = 'w' + ++this.idCounter;
      const ds = this.datasets.find((d) => d.id === wCfg.dataset);
      const dims = (wCfg.dimensions || [])
        .map((k: string) => ds?.fields.find((f) => f.key === k))
        .filter(Boolean);
      const msrs = (wCfg.measures || [])
        .map((k: string) => ds?.fields.find((f) => f.key === k))
        .filter(Boolean);
      const w: WidgetConfig = {
        id: wid,
        type: wCfg.type,
        title: wCfg.title,
        dataset: wCfg.dataset,
        dimensions: dims,
        measures: msrs,
        aggregations: wCfg.aggregations || {},
        filters: wCfg.filters || [],
        span: wCfg.span || 6,
        page: 0,
        pageSize: 20,
        showDataView: false,
        showQueryResults: false,
      };
      this.widgets = [...this.widgets, w];
      this.runWidget(w);
    }
  }

  // ── Field Panel ───────────────────────────────────────────
  filterFields(q: string) {
    if (!q) {
      this.filteredDatasets = this.datasets;
      return;
    }
    const lq = q.toLowerCase();
    this.filteredDatasets = this.datasets
      .map((ds) => ({
        ...ds,
        fields: ds.fields.filter((f) => f.label.toLowerCase().includes(lq)),
      }))
      .filter((ds) => ds.fields.length > 0);
    this.filteredDatasets.forEach((ds) => this.openDatasets.add(ds.id));
  }

  toggleDs(id: string) {
    this.openDatasets.has(id)
      ? this.openDatasets.delete(id)
      : this.openDatasets.add(id);
  }
  isOpen(id: string) {
    return this.openDatasets.has(id);
  }
  typeClass(t: string) {
    return (
      (
        {
          dim: 'ft-dim',
          msr: 'ft-msr',
          date: 'ft-date',
          bool: 'ft-bool',
        } as any
      )[t] || 'ft-dim'
    );
  }
  typeLabel(t: string) {
    return ({ dim: 'D', msr: 'M', date: 'T', bool: 'B' } as any)[t] || 'D';
  }

  // ── Drag & Drop ───────────────────────────────────────────
  onFieldDragStart(e: DragEvent, f: Field, dsId: string) {
    this.draggingField = { ...f, ds: dsId };
    e.dataTransfer!.effectAllowed = 'copy';
  }
  onFieldDragEnd() {
    this.draggingField = null;
  }
  allowDrop(e: DragEvent) {
    e.preventDefault();
  }

  // ── Auto-Run (debounced) ─────────────────────────────────
  scheduleAutoRun(w: WidgetConfig) {
    if (!w.dataset || (!w.dimensions.length && !w.measures.length)) return;
    const existing = this.autoRunTimers.get(w.id);
    if (existing) clearTimeout(existing);
    this.autoRunTimers.set(
      w.id,
      setTimeout(() => {
        this.autoRunTimers.delete(w.id);
        this.runWidget(w);
      }, 600),
    );
  }

  onCanvasDrop(e: DragEvent) {
    e.preventDefault();
    if (!this.draggingField) return;
    const type = this.draggingField.type === 'msr' ? 'kpi' : 'bar';
    this.addWidget(type, this.draggingField);
    // Auto-run the newly created widget
    const w = this.widgets[this.widgets.length - 1];
    if (w) this.scheduleAutoRun(w);
  }

  onWidgetDrop(e: DragEvent, wid: string) {
    e.preventDefault();
    e.stopPropagation();
    if (!this.draggingField) return;
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    this._addFieldToWidget(w, this.draggingField);
    this.showToast(`"${this.draggingField.label}" added to widget`);
    this.scheduleAutoRun(w);
  }

  onAxisDrop(e: DragEvent, wid: string, axis: 'x' | 'y') {
    e.preventDefault();
    e.stopPropagation();
    if (!this.draggingField) return;
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    if (axis === 'y') {
      if (!w.measures.find((f) => f.key === this.draggingField!.key)) {
        w.measures = [...w.measures, { ...this.draggingField }];
        if (!w.dataset) w.dataset = this.draggingField.ds;
      }
    } else {
      if (!w.dimensions.find((f) => f.key === this.draggingField!.key)) {
        w.dimensions = [...w.dimensions, { ...this.draggingField }];
        if (!w.dataset) w.dataset = this.draggingField.ds;
      }
    }
    this.scheduleAutoRun(w);
  }

  private _addFieldToWidget(w: WidgetConfig, f: Field & { ds?: string }) {
    if (f.type === 'msr') {
      if (!w.measures.find((x) => x.key === f.key)) {
        w.measures = [...w.measures, { ...f }];
        if (!w.dataset && f.ds) w.dataset = f.ds;
      }
    } else {
      if (!w.dimensions.find((x) => x.key === f.key)) {
        w.dimensions = [...w.dimensions, { ...f }];
        if (!w.dataset && f.ds) w.dataset = f.ds;
      }
    }
  }

  // ── Widgets ───────────────────────────────────────────────
  addWidget(type: WidgetConfig['type'], hint?: Field & { ds?: string }) {
    const id = 'w' + ++this.idCounter;
    const titles: any = {
      bar: 'Bar Chart',
      line: 'Line Chart',
      pie: 'Pie Chart',
      kpi: 'KPI Card',
      grid: 'Data Grid',
      hbar: 'Horizontal Bar',
      area: 'Area Chart',
      stacked: 'Stacked Bar',
      doughnut: 'Doughnut Chart',
    };
    const w: WidgetConfig = {
      id,
      type,
      title: hint ? hint.label + ' Analysis' : titles[type],
      dataset: (hint as any)?.ds || '',
      dimensions: hint && hint.type !== 'msr' ? [hint] : [],
      measures: hint && hint.type === 'msr' ? [hint] : [],
      aggregations: {},
      filters: [],
      span: 6,
      page: 0,
      pageSize: 20,
      showDataView: false,
      showQueryResults: false,
    };
    this.widgets = [...this.widgets, w];
    this.selectWidget(id);
  }

  removeWidget(id: string) {
    const ch = this.charts.get(id);
    if (ch) {
      ch.destroy();
      this.charts.delete(id);
    }
    this.widgets = this.widgets.filter((w) => w.id !== id);
    if (this.selectedId === id) this.selectedId = null;
  }

  duplicateWidget(wid: string) {
    const src = this.widgets.find((x) => x.id === wid);
    if (!src) return;
    const id = 'w' + ++this.idCounter;
    const w: WidgetConfig = {
      ...src,
      id,
      title: src.title + ' (copy)',
      dimensions: [...src.dimensions],
      measures: [...src.measures],
      aggregations: { ...src.aggregations },
      filters: src.filters.map((f) => ({ ...f })),
      data: src.data ? { ...src.data } : undefined,
      page: 0,
      showDataView: false,
      showQueryResults: false,
    };
    this.widgets = [...this.widgets, w];
    this.selectWidget(id);
    if (w.data && w.type !== 'kpi' && w.type !== 'grid') {
      setTimeout(() => this.renderChart(id), 80);
    }
    this.showToast('Widget duplicated');
  }

  selectWidget(id: string) {
    this.selectedId = id;
  }
  getSelected(): WidgetConfig | undefined {
    return this.widgets.find((w) => w.id === this.selectedId);
  }

  clearCanvas() {
    this.charts.forEach((c) => c.destroy());
    this.charts.clear();
    this.widgets = [];
    this.selectedId = null;
  }

  cycleSpan(w: WidgetConfig) {
    const spans = [3, 4, 6, 8, 12];
    w.span = spans[(spans.indexOf(w.span) + 1) % spans.length];
    setTimeout(() => this.renderChart(w.id), 80);
  }

  removeField(wid: string, key: string, axis: 'x' | 'y') {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    if (axis === 'x') w.dimensions = w.dimensions.filter((f) => f.key !== key);
    else {
      w.measures = w.measures.filter((f) => f.key !== key);
      delete w.aggregations[key];
    }
    this.scheduleAutoRun(w);
  }

  updateWidgetType(wid: string, type: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    w.type = type as any;
    w.showDataView = false;
    const ch = this.charts.get(wid);
    if (ch) {
      ch.destroy();
      this.charts.delete(wid);
    }
    if (w.data) setTimeout(() => this.renderChart(wid), 80);
    else this.scheduleAutoRun(w);
  }

  updateWidgetDataset(wid: string, dsId: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    w.dataset = dsId;
    w.dimensions = [];
    w.measures = [];
    w.aggregations = {};
    w.filters = [];
    w.data = undefined;
    w.warning = undefined;
  }

  getSpanClass(span: number) {
    return 'span-' + span;
  }
  getDsLabel(dsId: string) {
    return this.datasets.find((d) => d.id === dsId)?.label || '—';
  }

  // ── Aggregation ────────────────────────────────────────────
  getAgg(w: WidgetConfig, key: string): string {
    return w.aggregations[key] || 'SUM';
  }
  setAgg(w: WidgetConfig, key: string, agg: string) {
    w.aggregations[key] = agg;
    this.scheduleAutoRun(w);
  }

  // ── Filters ────────────────────────────────────────────────
  addFilter(wid: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    w.filters = [...w.filters, { field: '', op: '=', value: '' }];
  }
  removeFilter(wid: string, idx: number) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    w.filters = w.filters.filter((_, i) => i !== idx);
  }
  getFieldsForDataset(dsId: string): Field[] {
    return this.datasets.find((d) => d.id === dsId)?.fields || [];
  }

  // ── Query ─────────────────────────────────────────────────
  runWidget(w: WidgetConfig) {
    if (!w.dataset || (!w.dimensions.length && !w.measures.length)) {
      this.showToast('Add at least one field first');
      return;
    }
    w.loading = true;
    w.error = undefined;
    w.warning = undefined;

    const req: QueryRequest = {
      dataset: w.dataset,
      dimensions: w.dimensions.map((f) => f.key),
      measures: w.measures.map((f) => f.key),
      limit: w.type === 'grid' ? 100 : 25,
      order_dir: 'DESC',
    };
    if (w.measures.length) req.order_by = w.measures[0].key;

    // Aggregations (only send non-default)
    const aggs: Record<string, string> = {};
    for (const m of w.measures) {
      const a = w.aggregations[m.key];
      if (a && a !== 'SUM') aggs[m.key] = a;
    }
    if (Object.keys(aggs).length) req.aggregations = aggs;

    // Join dataset (cross-table)
    if (w.joinDataset) req.join_dataset = w.joinDataset;

    // Filters (only send complete ones)
    const validFilters = w.filters.filter((f) => f.field && f.value);
    if (validFilters.length) {
      req.filters = validFilters.map((f) => {
        if (f.op === 'IN') {
          return {
            field: f.field,
            op: f.op,
            value: f.value.split(',').map((v) => v.trim()),
          };
        }
        return { field: f.field, op: f.op, value: f.value };
      });
    }
    console.log('Query Request:', req);
    this.api.query(req).subscribe({
      next: (res) => {
        w.loading = false;
        w.data = res;
        w.page = 0;
        w.warning = (res as any).chart?.warning || undefined;
        if (w.type !== 'kpi' && w.type !== 'grid') {
          setTimeout(() => this.renderChart(w.id), 80);
        }
      },
      error: (err) => {
        w.loading = false;
        w.error = err.error?.detail || 'Query failed';
      },
    });
  }

  runAll() {
    this.runningAll = true;
    this.widgets.forEach((w) => this.runWidget(w));
    setTimeout(() => {
      this.runningAll = false;
      this.showToast('All widgets refreshed');
    }, 1400);
  }

  // ── CLIENT-SIDE CHART DATA CONSTRUCTION ────────────────────
  private buildChartData(w: WidgetConfig): {
    labels: string[];
    datasets: any[];
    dualAxis: boolean;
  } {
    const rows = w.data?.rows || [];
    if (!rows.length) return { labels: [], datasets: [], dualAxis: false };

    // Composite labels from dimensions
    const labels = rows.map((r: any) => {
      if (!w.dimensions.length) return 'Value';
      return w.dimensions
        .map((d) => String(r[d.key] || ''))
        .filter(Boolean)
        .join(' / ');
    });

    // Dual-axis detection
    let dualAxis = false;
    if (w.measures.length >= 2) {
      const mags = w.measures.map((m) => {
        const vals = rows
          .map((r: any) => Math.abs(parseFloat(r[m.key]) || 0))
          .filter((v: number) => v > 0);
        if (!vals.length) return 0;
        const avg =
          vals.reduce((a: number, b: number) => a + b, 0) / vals.length;
        return Math.log10(avg + 1);
      });
      const baseMag = mags[0];
      dualAxis = mags.some((m) => Math.abs(m - baseMag) > 1.5);
    }

    // Build datasets
    const datasets = w.measures.map((m, i) => {
      const data = rows.map((r: any) => parseFloat(r[m.key]) || 0);
      const color = COLORS[i % COLORS.length];
      const ds: any = {
        label: m.label,
        data,
        backgroundColor: color + 'cc',
        borderColor: color,
        borderWidth: 2,
        borderRadius: 4,
      };
      if (dualAxis) {
        ds.yAxisID = i === 0 ? 'y' : 'y1';
      }
      return ds;
    });

    return { labels, datasets, dualAxis };
  }

  // ── CHART RENDERER ────────────────────────────────────────
  renderChart(id: string) {
    const w = this.widgets.find((x) => x.id === id);
    if (!w || !w.data || w.type === 'kpi' || w.type === 'grid') return;

    const canvas = document.getElementById('canvas-' + id) as HTMLCanvasElement;
    if (!canvas) return;

    const old = this.charts.get(id);
    if (old) {
      old.destroy();
      this.charts.delete(id);
    }

    Chart.defaults.color = '#94a3b8';
    Chart.defaults.borderColor = '#252a3d';
    Chart.defaults.font.family = 'Outfit';
    Chart.defaults.font.size = 11;

    // Build chart data client-side from rows
    const { labels, datasets: rawDatasets, dualAxis } = this.buildChartData(w);
    if (!labels.length) return;

    const multiMeasure = rawDatasets.length > 1;

    // ── PIE / DOUGHNUT ─────────────────────────────────────
    if (w.type === 'pie' || w.type === 'doughnut') {
      const data = rawDatasets[0]?.data || [];
      this.charts.set(
        id,
        new Chart(canvas, {
          type: 'doughnut',
          data: {
            labels,
            datasets: [
              {
                data,
                backgroundColor: COLORS,
                borderWidth: 2,
                borderColor: '#13161f',
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: w.type === 'pie' ? '0%' : '65%',
            plugins: {
              legend: {
                position: 'right',
                labels: { boxWidth: 10, padding: 10, font: { size: 10 } },
              },
            },
          },
        }),
      );
      return;
    }

    // ── LINE / AREA ──────────────────────────────────────────
    if (w.type === 'line' || w.type === 'area') {
      const isArea = w.type === 'area';
      const cjsDatasets: ChartDataset<'line'>[] = rawDatasets.map(
        (ds: any, i: number) => ({
          label: ds.label,
          data: ds.data,
          borderColor: COLORS[i % COLORS.length],
          backgroundColor: COLORS[i % COLORS.length] + (isArea ? '33' : '18'),
          tension: 0.4,
          fill: isArea || i === 0,
          pointRadius: 3,
          pointBackgroundColor: COLORS[i % COLORS.length],
          pointBorderColor: '#07080f',
          pointBorderWidth: 2,
          yAxisID: dualAxis ? (i === 0 ? 'y' : 'y1') : 'y',
        }),
      );

      const scales: any = {
        x: { grid: { color: '#252a3d' } },
        y: {
          grid: { color: '#252a3d' },
          position: 'left' as const,
          title: {
            display: dualAxis,
            text: w.measures[0]?.label || '',
            color: COLORS[0],
            font: { size: 10 },
          },
        },
      };
      if (dualAxis) {
        scales['y1'] = {
          grid: { drawOnChartArea: false },
          position: 'right' as const,
          title: {
            display: true,
            text: w.measures[1]?.label || '',
            color: COLORS[1],
            font: { size: 10 },
          },
        };
      }

      this.charts.set(
        id,
        new Chart(canvas, {
          type: 'line',
          data: { labels, datasets: cjsDatasets },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: {
                display: multiMeasure,
                labels: { boxWidth: 10, padding: 10, font: { size: 10 } },
              },
            },
            scales,
          },
        }),
      );
      return;
    }

    // ── BAR / HBAR / STACKED ──────────────────────────────────
    const isHorizontal = w.type === 'hbar';
    const isStacked = w.type === 'stacked';

    const cjsDatasets: ChartDataset<'bar'>[] = rawDatasets.map(
      (ds: any, i: number) => ({
        label: ds.label,
        data: ds.data,
        backgroundColor: COLORS[i % COLORS.length] + 'cc',
        borderColor: COLORS[i % COLORS.length],
        borderWidth: 1,
        borderRadius: 4,
        borderSkipped: false,
        yAxisID: dualAxis && !isStacked ? (i === 0 ? 'y' : 'y1') : 'y',
      }),
    );

    const scales: any = {
      x: {
        grid: { display: false },
        stacked: isStacked,
      },
      y: {
        grid: { color: '#252a3d' },
        position: 'left' as const,
        stacked: isStacked,
        title: {
          display: dualAxis && !isStacked,
          text: w.measures[0]?.label || '',
          color: COLORS[0],
          font: { size: 10 },
        },
      },
    };
    if (dualAxis && !isStacked) {
      scales['y1'] = {
        grid: { drawOnChartArea: false },
        position: 'right' as const,
        title: {
          display: true,
          text: w.measures[1]?.label || '',
          color: COLORS[1],
          font: { size: 10 },
        },
      };
    }

    this.charts.set(
      id,
      new Chart(canvas, {
        type: 'bar',
        data: { labels, datasets: cjsDatasets },
        options: {
          indexAxis: isHorizontal ? 'y' : 'x',
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              display: multiMeasure,
              labels: { boxWidth: 10, padding: 10, font: { size: 10 } },
            },
            tooltip: {
              callbacks: {
                label: (ctx: any) => {
                  const v = isHorizontal ? ctx.parsed.x : ctx.parsed.y;
                  return ` ${ctx.dataset.label}: ${this.formatNumber(v)}`;
                },
              },
            },
          },
          scales,
        },
      }),
    );
  }

  // ── VALUE FORMATTING ──────────────────────────────────────
  formatNumber(v: number): string {
    if (v == null || isNaN(v)) return '—';
    if (Math.abs(v) >= 1_000_000)
      return 'LKR ' + (v / 1_000_000).toFixed(2) + 'M';
    if (Math.abs(v) >= 1_000) return v.toLocaleString();
    return String(Math.round(v * 100) / 100);
  }

  formatValue(value: any, field?: Field): string {
    if (value == null) return '—';
    const n = parseFloat(value);
    if (isNaN(n)) return String(value);
    if (!field) return this.formatNumber(n);
    const label = field.label.toLowerCase();
    if (
      label.includes('lkr') ||
      label.includes('revenue') ||
      label.includes('spend') ||
      label.includes('price') ||
      label.includes('uplift')
    ) {
      if (Math.abs(n) >= 1_000_000)
        return 'LKR ' + (n / 1_000_000).toFixed(2) + 'M';
      if (Math.abs(n) >= 1_000) return 'LKR ' + n.toLocaleString();
      return 'LKR ' + n.toFixed(2);
    }
    if (label.includes('%') || label.includes('pct') || label.includes('roi')) {
      return n.toFixed(1) + '%';
    }
    return n.toLocaleString();
  }

  // ── KPI ───────────────────────────────────────────────────
  getKpiValue(w: WidgetConfig): string {
    if (!w.data?.rows?.length) return '—';
    const r = w.data.rows[0];
    const m = w.measures[0];
    const key = m?.key;
    const v = key ? r[key] : null;
    if (v == null) return '—';
    return this.formatValue(v, m);
  }

  getKpiTrend(w: WidgetConfig): {
    direction: 'up' | 'down' | 'flat';
    pct: number;
  } {
    if (!w.data?.rows?.length || w.data.rows.length < 2 || !w.measures.length) {
      return { direction: 'flat', pct: 0 };
    }
    const key = w.measures[0].key;
    const rows = w.data.rows;
    const mid = Math.floor(rows.length / 2);
    const firstHalf =
      rows
        .slice(0, mid)
        .reduce((a: number, r: any) => a + (parseFloat(r[key]) || 0), 0) / mid;
    const secondHalf =
      rows
        .slice(mid)
        .reduce((a: number, r: any) => a + (parseFloat(r[key]) || 0), 0) /
      (rows.length - mid);
    if (firstHalf === 0) return { direction: 'flat', pct: 0 };
    const pct = Math.round(
      ((secondHalf - firstHalf) / Math.abs(firstHalf)) * 100,
    );
    return {
      direction: pct > 0 ? 'up' : pct < 0 ? 'down' : 'flat',
      pct: Math.abs(pct),
    };
  }

  getMsrSummary(
    w: WidgetConfig,
  ): { label: string; value: string; color: string }[] {
    if (!w.data?.rows?.length) return [];
    return w.measures.map((m, i) => {
      const sum = w.data!.rows.reduce(
        (acc: number, r: any) => acc + (parseFloat(r[m.key]) || 0),
        0,
      );
      return {
        label: m.label,
        value: this.formatValue(sum, m),
        color: COLORS[i % COLORS.length],
      };
    });
  }

  // ── GRID SORTING & PAGINATION ─────────────────────────────
  sortGrid(wid: string, col: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w || !w.data?.rows) return;
    if (w.sortCol === col) {
      w.sortDir = w.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      w.sortCol = col;
      w.sortDir = 'desc';
    }
    const dir = w.sortDir === 'asc' ? 1 : -1;
    const isMsr = this.isMeasure(w, col);
    w.data.rows.sort((a: any, b: any) => {
      const va = a[col],
        vb = b[col];
      if (isMsr) return (parseFloat(va) - parseFloat(vb)) * dir;
      return String(va || '').localeCompare(String(vb || '')) * dir;
    });
    w.page = 0;
  }

  getPagedRows(w: WidgetConfig): any[] {
    if (!w.data?.rows) return [];
    const start = w.page * w.pageSize;
    return w.data.rows.slice(start, start + w.pageSize);
  }

  gridPageCount(w: WidgetConfig): number {
    if (!w.data?.rows) return 0;
    return Math.ceil(w.data.rows.length / w.pageSize);
  }

  gridNextPage(wid: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    if (w.page < this.gridPageCount(w) - 1) w.page++;
  }

  gridPrevPage(wid: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    if (w.page > 0) w.page--;
  }

  formatCell(value: any, col: string, w: WidgetConfig): string {
    if (value == null) return '—';
    if (!this.isMeasure(w, col)) return String(value);
    const field = w.measures.find((m) => m.key === col);
    return this.formatValue(value, field);
  }

  // ── FILTER AUTOCOMPLETE ─────────────────────────────────────
  loadFilterValues(dsId: string, fieldKey: string) {
    const cacheKey = `${dsId}:${fieldKey}`;
    console.log('Loading filter values for', cacheKey);
    if (this.filterValuesCache.has(cacheKey)) return;
    this.filterValuesLoading = true;
    this.api.getDistinctValues(dsId, fieldKey).subscribe({
      next: (vals) => {
        this.filterValuesCache.set(cacheKey, vals);
        this.filterValuesLoading = false;
      },
      error: () => {
        this.filterValuesLoading = false;
      },
    });
  }

  getFilterValues(dsId: string, fieldKey: string): string[] {
    const cacheKey = `${dsId}:${fieldKey}`;
    console.log('Getting filter values for', cacheKey);
    return this.filterValuesCache.get(cacheKey) || [];
  }

  // onFilterFieldChange(w: WidgetConfig, idx: number) {
  //   const f = w.filters[idx];
  //   if (f.field && w.dataset) {
  //     this.loadFilterValues(w.dataset, f.field);
  //   }
  // }

  onFilterFieldChange(w: WidgetConfig, idx: number) {
    const f = w.filters[idx];
    if (f.field && w.dataset) {
      this.loadFilterValues(w.dataset, f.field);
      // Clear the value when field changes
      f.value = '';
    }
  }

  onFilterValueChange(w: WidgetConfig, idx: number) {
    const f = w.filters[idx];
    if (f.field && f.value) {
      this.scheduleAutoRun(w);
    }
  }

  // ── JOIN / CROSS-TABLE ─────────────────────────────────────
  getJoinableDatasets(dsId: string): { id: string; label: string }[] {
    return this.relationships
      .filter((r) => r.from === dsId || r.join === dsId)
      .map((r) => {
        const joinId = r.from === dsId ? r.join : r.from;
        const joinLabel = r.from === dsId ? r.joinLabel : r.fromLabel;
        return { id: joinId, label: joinLabel };
      });
  }

  getJoinedFields(w: WidgetConfig): Field[] {
    if (!w.joinDataset) return [];
    return this.datasets.find((d) => d.id === w.joinDataset)?.fields || [];
  }

  setJoinDataset(w: WidgetConfig, dsId: string) {
    w.joinDataset = dsId || undefined;
  }

  // ── SAVE / LOAD DASHBOARDS ────────────────────────────────
  openSaveDialog() {
    if (!this.dashboardName && this.widgets.length) {
      this.dashboardName = 'My Report';
    }
    this.showSaveDialog = true;
  }

  closeSaveDialog() {
    this.showSaveDialog = false;
  }

  saveDashboard() {
    if (!this.dashboardName.trim()) return;
    this.builderService
      .saveDashboardToDb(
        this.dashboardName,
        this.widgets,
        this.currentDashboardId || undefined,
      )
      .subscribe({
        next: (res) => {
          this.currentDashboardId = res.id;
          this.showSaveDialog = false;
          this.showToast(`"${this.dashboardName}" saved to database`);
        },
        error: () => this.showToast('Save failed'),
      });
  }

  openLoadDialog() {
    this.builderService.getBuilderDashboardsFromDb().subscribe({
      next: (dashboards) => {
        this.savedDashboards = dashboards;
        this.showLoadDialog = true;
      },
    });
  }

  closeLoadDialog() {
    this.showLoadDialog = false;
  }

  loadDashboard(id: string) {
    this.builderService.loadDashboardFromDb(id).subscribe({
      next: (db) => {
        this._loadDashboardData(db);
        this.showLoadDialog = false;
        this.showToast(`"${db.name}" loaded`);
      },
      error: () => this.showToast('Load failed'),
    });
  }

  deleteSavedDashboard(id: string) {
    this.builderService.deleteBuilderDashboardFromDb(id).subscribe({
      next: () => {
        this.savedDashboards = this.savedDashboards.filter((d) => d.id !== id);
        if (this.currentDashboardId === id) this.currentDashboardId = null;
      },
    });
  }

  // ── Preset loader ─────────────────────────────────────────
  loadPresetIfAny(): boolean {
    const raw = sessionStorage.getItem('loadPreset');
    if (!raw) return false;
    sessionStorage.removeItem('loadPreset');
    const preset = JSON.parse(raw);
    preset.widgets.forEach((wCfg: any) => {
      const id = 'w' + ++this.idCounter;
      const ds = this.datasets.find((d: any) => d.id === wCfg.dataset);
      const dims = wCfg.dimensions
        .map((k: string) => ds?.fields.find((f: any) => f.key === k))
        .filter(Boolean);
      const msrs = wCfg.measures
        .map((k: string) => ds?.fields.find((f: any) => f.key === k))
        .filter(Boolean);
      const w: WidgetConfig = {
        id,
        type: wCfg.type,
        title: wCfg.title,
        dataset: wCfg.dataset,
        dimensions: dims,
        measures: msrs,
        aggregations: {},
        filters: [],
        span: 6,
        page: 0,
        pageSize: 20,
        showDataView: false,
        showQueryResults: false,
      };
      this.widgets = [...this.widgets, w];
      this.runWidget(w);
    });
    this.showToast(`"${preset.name}" loaded!`);
    return true;
  }

  // ── DATA VIEW TOGGLE & CSV EXPORT ───────────────────────
  private savedSpans = new Map<string, number>();

  toggleDataView(wid: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    w.showDataView = !w.showDataView;
    if (w.showDataView) {
      // Expand to full width for readability
      this.savedSpans.set(wid, w.span);
      if (w.span < 8) w.span = 12;
    } else {
      // Restore original span
      const saved = this.savedSpans.get(wid);
      if (saved) {
        w.span = saved;
        this.savedSpans.delete(wid);
      }
      if (w.data && w.type !== 'kpi' && w.type !== 'grid') {
        setTimeout(() => this.renderChart(wid), 80);
      }
    }
  }

  downloadCsv(wid: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w?.data?.rows?.length) return;

    const cols = w.data.columns as string[];
    const escCsv = (v: any): string => {
      const s = v == null ? '' : String(v);
      return s.includes(',') || s.includes('"') || s.includes('\n')
        ? '"' + s.replace(/"/g, '""') + '"'
        : s;
    };

    const header = cols.map(escCsv).join(',');
    const rows = w.data.rows.map((r: any) =>
      cols.map((c) => escCsv(r[c])).join(','),
    );
    const csv = [header, ...rows].join('\n');

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = (w.title || 'export').replace(/\s+/g, '_') + '.csv';
    a.click();
    URL.revokeObjectURL(url);
    this.showToast('CSV downloaded');
  }

  showToast(msg: string) {
    this.toast = msg;
    setTimeout(() => (this.toast = ''), 2800);
  }

  isMeasure(w: WidgetConfig, col: string): boolean {
    return !!w.measures?.some((m) => m.key === col);
  }

  getColLabel(w: WidgetConfig, col: string): string {
    const dim = w.dimensions.find((f) => f.key === col);
    if (dim) return dim.label;
    const msr = w.measures.find((f) => f.key === col);
    if (msr) return msr.label;
    return col;
  }

  // ── CHART EXPORT AS PNG ────────────────────────────────────
  downloadChartPng(wid: string) {
    const w = this.widgets.find((x) => x.id === wid);
    if (!w) return;
    const chart = this.charts.get(wid);
    if (!chart) return;
    const url = chart.toBase64Image('image/png', 1);
    const a = document.createElement('a');
    a.href = url;
    a.download = (w.title || 'chart').replace(/\s+/g, '_') + '.png';
    a.click();
    this.showToast('Chart image downloaded');
  }
}
