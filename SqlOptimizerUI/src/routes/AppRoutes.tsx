import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from '../core/layouts/MainLayout';
import { DashboardPage } from '../features/dashboard/DashboardPage';
import { ConnectionPage } from '../features/connection/ConnectionPage';
import { ExplorerPage } from '../features/explorer/ExplorerPage';
import { ProceduresPage } from '../features/procedures/ProceduresPage';
import { ProcedureDetailsPage } from '../features/procedures/ProcedureDetailsPage';
import { AIAnalysisPage } from '../features/ai-analysis/AIAnalysisPage';
import { DatabaseAnalysisPage } from '../features/analysis/DatabaseAnalysisPage';
import { ComparisonPage } from '../features/comparison/ComparisonPage';
import { ReportsPage } from '../features/reports/ReportsPage';
import { SettingsPage } from '../features/settings/SettingsPage';

export const AppRoutes = () => {
  return (
    <BrowserRouter>
      <MainLayout>
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/connection" element={<ConnectionPage />} />
          <Route path="/explorer" element={<ExplorerPage />} />
          <Route path="/procedures" element={<ProceduresPage />} />
          <Route path="/procedures/:procedureName" element={<ProcedureDetailsPage />} />
          <Route path="/ai-analysis" element={<AIAnalysisPage />} />
          <Route path="/database-analysis" element={<DatabaseAnalysisPage />} />
          <Route path="/comparison" element={<ComparisonPage />} />
          <Route path="/reports" element={<ReportsPage />} />
          <Route path="/settings" element={<SettingsPage />} />
        </Routes>
      </MainLayout>
    </BrowserRouter>
  );
};
