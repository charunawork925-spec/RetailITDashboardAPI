// analytics-builder.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';


type WidgetType = 'line' | 'bar' | 'pie' | 'radar' | 'doughnut' | 'gauge' | 'kpi' | 'table';
type WidgetSize = 'small' | 'medium' | 'large' | 'xlarge';

export interface DashboardWidget {
  id: string;
  title: string;
  type: WidgetType;
  size: WidgetSize;
  dataSource: string;
  refreshInterval: number;
  position: { x: number; y: number; cols: number; rows: number };
  config: any;
  lastUpdated: Date;
}

export interface UserDashboard {
  id: string;
  name: string;
  description?: string;
  widgets: DashboardWidget[];
  createdAt: Date;
  updatedAt: Date;
}

export interface AnalyticsTopic {
  id: string;
  name: string;
  description: string;
  category: 'sales' | 'inventory' | 'customers' | 'promotion' | 'profitability' | 'wastage';
  dataSource: string;
  availableWidgets: string[];
  defaultConfig?: any;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsBuilderService {
  private availableTopics: AnalyticsTopic[] = [
    {
      id: 'sales-overview',
      name: 'Sales Overview',
      description: 'Daily, weekly, monthly sales metrics',
      category: 'sales',
      dataSource: 'sales',
      availableWidgets: ['metric', 'line-chart', 'bar-chart']
    },
    {
      id: 'customer-analytics',
      name: 'Customer Analytics',
      description: 'Customer behavior and segmentation',
      category: 'customers',
      dataSource: 'customers',
      availableWidgets: ['metric', 'pie-chart', 'table']
    },
    {
      id: 'inventory-status',
      name: 'Inventory Status',
      description: 'Stock levels and alerts',
      category: 'inventory',
      dataSource: 'inventory',
      availableWidgets: ['metric', 'gauge', 'list']
    },
    {
      id: 'promotion-effectiveness',
      name: 'Promotion Effectiveness',
      description: 'Promotion ROI and performance',
      category: 'promotion',
      dataSource: 'promotions',
      availableWidgets: ['metric', 'bar-chart', 'comparison-chart']
    },
    {
      id: 'profitability-analysis',
      name: 'Profitability Analysis',
      description: 'Profit margins by product/category',
      category: 'profitability',
      dataSource: 'profitability',
      availableWidgets: ['metric', 'donut-chart', 'tree-map']
    },
    {
      id: 'wastage-tracking',
      name: 'Wastage Tracking',
      description: 'Inventory wastage and loss',
      category: 'wastage',
      dataSource: 'wastage',
      availableWidgets: ['metric', 'line-chart', 'table']
    }
  ];

  private base = 'http://localhost:8000/api';
  private userDashboards: UserDashboard[] = [];
  private currentDashboard = new BehaviorSubject<UserDashboard | null>(null);

  constructor(private http: HttpClient) {
    this.loadDashboards();
  }

  getAvailableTopics(): AnalyticsTopic[] {
    return this.availableTopics;
  }

  getTopicsByCategory(category: string): AnalyticsTopic[] {
    return this.availableTopics.filter(topic => topic.category === category);
  }

  createDashboard(name: string, description?: string): UserDashboard {
    const dashboard: UserDashboard = {
      id: this.generateId(),
      name,
      description,
      widgets: [],
      createdAt: new Date(),
      updatedAt: new Date()
    };
    
    this.userDashboards.push(dashboard);
    this.saveDashboards();
    return dashboard;
  }

  addWidgetToDashboard(dashboardId: string, widget: DashboardWidget): void {
    const dashboard = this.userDashboards.find(d => d.id === dashboardId);
    if (dashboard) {
      dashboard.widgets.push(widget);
      dashboard.updatedAt = new Date();
      this.saveDashboards();
    }
  }

  updateWidgetPosition(dashboardId: string, widgetId: string, position: any): void {
    const dashboard = this.userDashboards.find(d => d.id === dashboardId);
    if (dashboard) {
      const widget = dashboard.widgets.find(w => w.id === widgetId);
      if (widget) {
        widget.position = position;
        dashboard.updatedAt = new Date();
        this.saveDashboards();
      }
    }
  }

  // ── Builder-compatible save/load (via PostgreSQL API) ────
  private serializeWidgets(widgets: any[]): any[] {
    return widgets.map(w => ({
      title: w.title,
      type: w.type,
      dataset: w.dataset,
      dimensions: w.dimensions.map((f: any) => f.key || f),
      measures: w.measures.map((f: any) => f.key || f),
      aggregations: w.aggregations || {},
      filters: w.filters || [],
      span: w.span,
    }));
  }

  saveDashboardToDb(name: string, widgets: any[], existingId?: string): Observable<any> {
    const serialized = this.serializeWidgets(widgets);
    if (existingId) {
      return this.http.put(`${this.base}/dashboards/${existingId}`, { name, widgets: serialized });
    }
    return this.http.post(`${this.base}/dashboards`, { name, widgets: serialized });
  }

  loadDashboardFromDb(id: string): Observable<any> {
    return this.http.get(`${this.base}/dashboards/${id}`);
  }

  getBuilderDashboardsFromDb(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/dashboards`);
  }

  deleteBuilderDashboardFromDb(id: string): Observable<any> {
    return this.http.delete(`${this.base}/dashboards/${id}`);
  }

  // Keep localStorage versions as fallback
  getBuilderDashboards(): any[] {
    const saved = localStorage.getItem('builderDashboards');
    return saved ? JSON.parse(saved) : [];
  }

  // ── Custom presets ─────────────────────────────────────────
  saveCustomPreset(preset: any): void {
    const presets = this.getCustomPresets();
    preset.id = preset.id || this.generateId();
    preset.createdAt = new Date().toISOString();
    presets.push(preset);
    localStorage.setItem('customPresets', JSON.stringify(presets));
  }

  getCustomPresets(): any[] {
    const saved = localStorage.getItem('customPresets');
    return saved ? JSON.parse(saved) : [];
  }

  deleteCustomPreset(id: string): void {
    const presets = this.getCustomPresets().filter((p: any) => p.id !== id);
    localStorage.setItem('customPresets', JSON.stringify(presets));
  }

  private saveDashboards(): void {
    localStorage.setItem('userDashboards', JSON.stringify(this.userDashboards));
  }

  private loadDashboards(): void {
    const saved = localStorage.getItem('userDashboards');
    if (saved) {
      this.userDashboards = JSON.parse(saved);
    }
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }
}