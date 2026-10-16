import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { RequireAuth } from './components/RequireAuth'
import { FeedbackFormPage } from './pages/FeedbackFormPage'
import { LoginPage } from './pages/LoginPage'
import { ManagerDashboardPage } from './pages/ManagerDashboardPage'
import { ManagerFeedbackPage } from './pages/ManagerFeedbackPage'
import { MyFeedbackPage } from './pages/MyFeedbackPage'
import { RegisterPage } from './pages/RegisterPage'

export default function App() {
  return (
    <BrowserRouter>
      <Layout>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/"
            element={
              <RequireAuth role="StoreManager">
                <ManagerDashboardPage />
              </RequireAuth>
            }
          />
          <Route
            path="/feedback/new"
            element={
              <RequireAuth role="Customer">
                <FeedbackFormPage />
              </RequireAuth>
            }
          />
          <Route
            path="/feedback/mine"
            element={
              <RequireAuth role="Customer">
                <MyFeedbackPage />
              </RequireAuth>
            }
          />
          <Route
            path="/manager/feedback"
            element={
              <RequireAuth role="StoreManager">
                <ManagerFeedbackPage />
              </RequireAuth>
            }
          />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  )
}
