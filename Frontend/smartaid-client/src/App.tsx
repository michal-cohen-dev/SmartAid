
import './App.css';
import { Routes, Route } from 'react-router-dom';

import RequesterHomeScreen from './components/RequesterHomeScreen';
import AppLayout from './components/AppLayout';
import MainDashboard from './components/MainDashboard';
import LoginScreen from './components/LoginScreen';
import RegistrationScreen from './components/RegistrationScreen';
import CreateRequestScreen from './components/CreateRequestScreen';
import AISuggestionsScreen from './components/AISuggestionsScreen';
import RequestDetailsScreen from './components/RequestDetailsScreen';
import RequestTrackingScreen from './components/RequestTrackingScreen';
import VolunteerHome from './components/VolunteerHome';
import VolunteerReg from './components/VolunteerReg';
import MyRequestsScreen from './components/MyRequestsScreen';
import NotFound from './NotFound';
import VolunteerHistoryScreen from './components/VolunteerHistoryScreen';
import VolunteerAcceptedRequestsScreen from './components/VolunteerAcceptedRequestsScreen';
import VolunteerCompletedRequestsScreen from './components/VolunteerCompletedRequestsScreen';
import ProfileScreen from './components/ProfileScreen';

import ProtectedRoute from './routes/ProtectedRoute';

import AdminLayout from './admin/AdminLayout';
import AdminDashboard from './admin/AdminDashboard';

function App() {
  return (
    <Routes>
      {/* מסכים ציבוריים */}
      <Route path="/" element={<MainDashboard />} />
      <Route path="/login" element={<LoginScreen />} />
      <Route path="/register" element={<RegistrationScreen />} />

      {/* כל המסכים הפנימיים דורשים התחברות ומקבלים את התפריט העליון */}
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        {/* מסכי מתנדב */}
        <Route
          path="/volunteer"
          element={
            <ProtectedRoute requireVolunteer>
              <VolunteerHome />
            </ProtectedRoute>
          }
        />

        <Route
          path="/volunteer/open-requests"
          element={
            <ProtectedRoute requireVolunteer>
              <VolunteerHome />
            </ProtectedRoute>
          }
        />

        <Route
          path="/volunteer/history"
          element={
            <ProtectedRoute requireVolunteer>
              <VolunteerHistoryScreen />
            </ProtectedRoute>
          }
        />

        <Route
          path="/volunteer/accepted-requests"
          element={
            <ProtectedRoute requireVolunteer>
              <VolunteerAcceptedRequestsScreen />
            </ProtectedRoute>
          }
        />

        <Route
          path="/volunteer/completed-requests"
          element={
            <ProtectedRoute requireVolunteer>
              <VolunteerCompletedRequestsScreen />
            </ProtectedRoute>
          }
        />

        <Route
          path="/request-details/:id"
          element={
            <ProtectedRoute requireVolunteer>
              <RequestDetailsScreen />
            </ProtectedRoute>
          }
        />

        {/* מסכי משתמש */}
        <Route
          path="/volunteer-registration"
          element={<VolunteerReg />}
        />

        <Route
          path="/requester"
          element={<RequesterHomeScreen />}
        />

        <Route
          path="/request"
          element={<CreateRequestScreen />}
        />

        <Route
          path="/suggestions"
          element={<AISuggestionsScreen />}
        />

        <Route
          path="/suggestions/:id"
          element={<AISuggestionsScreen />}
        />

        <Route
          path="/request-tracking/:id"
          element={<RequestTrackingScreen />}
        />

        <Route
          path="/my-requests"
          element={<MyRequestsScreen />}
        />

        <Route
          path="/profile"
          element={<ProfileScreen />}
        />
      </Route>
      <Route
  path="/admin"
  element={
    <ProtectedRoute requireAdmin>
      <AdminLayout />
    </ProtectedRoute>
  }
>
  <Route index element={<AdminDashboard />} />
</Route>

      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

export default App;
