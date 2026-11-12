
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

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainDashboard />} />
      <Route path="/login" element={<LoginScreen />} />
      <Route path="/register" element={<RegistrationScreen />} />

      <Route element={<AppLayout />}>
        <Route path="/volunteer" element={<VolunteerHome />} />
        <Route path="/volunteer/open-requests" element={<VolunteerHome />} />
<Route
  path="/volunteer/history"
  element={<VolunteerHistoryScreen />}
/>
        <Route
          path="/volunteer-registration"
          element={<VolunteerReg />}
        />

        <Route path="/request-details/:id" element={<RequestDetailsScreen />} />
        <Route path="/requester" element={<RequesterHomeScreen />} />
        <Route path="/request" element={<CreateRequestScreen />} />
<Route path="/suggestions/:id" element={<AISuggestionsScreen />} /><Route path="/request-tracking/:id" element={<RequestTrackingScreen />} />
        <Route path="/my-requests" element={<MyRequestsScreen />} />
      </Route>

      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

export default App;
