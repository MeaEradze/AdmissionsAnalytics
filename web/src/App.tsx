import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { Toaster } from '@/components/ui/sonner'
import { AppShell } from '@/components/layout/AppShell'
import { ComparePage } from '@/pages/ComparePage'
import { DashboardPage } from '@/pages/DashboardPage'
import { DataPage } from '@/pages/DataPage'
import { HealthPage } from '@/pages/HealthPage'
import { MarketCompetitionPage } from '@/pages/MarketCompetitionPage'
import { ProgramAnalysisPage } from '@/pages/ProgramAnalysisPage'
import { TrendsPage } from '@/pages/TrendsPage'

export default function App() {
  return (
    <BrowserRouter>
      <Toaster position="top-center" richColors />
      <Routes>
        <Route element={<AppShell />}>
          <Route index element={<DashboardPage />} />
          <Route path="health" element={<HealthPage />} />
          <Route path="market" element={<MarketCompetitionPage />} />
          <Route path="program" element={<ProgramAnalysisPage />} />
          <Route path="trends" element={<TrendsPage />} />
          <Route path="compare" element={<ComparePage />} />
          <Route path="data" element={<DataPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
