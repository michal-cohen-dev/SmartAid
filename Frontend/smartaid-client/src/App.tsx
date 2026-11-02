
import './App.css'
import { Routes, Route } from 'react-router-dom'
import RequesterHomeScreen from './components/RequesterHomeScreen'
import AppLayout from './components/AppLayout'
import MainDashboard from './components/MainDashboard'
import LoginScreen from './components/LoginScreen'
import RegistrationScreen from './components/RegistrationScreen'
import CreateRequestScreen from './components/CreateRequestScreen'
import AISuggestionsScreen from './components/AISuggestionsScreen'
import RequestDetailsScreen from './components/RequestDetailsScreen'
import RequestTrackingScreen from './components/RequestTrackingScreen'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginScreen />} />
      <Route path="/register" element={<RegistrationScreen />} />

      <Route element={<AppLayout />}>
       
       
        <Route path="/" element={<MainDashboard />} />
        <Route path="/volunteer" element={<MainDashboard />} />
        <Route path="/request-details" element={<RequestDetailsScreen />} />

       
       <Route path="/requester" element={<RequesterHomeScreen />} />
        <Route path="/request" element={<CreateRequestScreen />} />
        <Route path="/suggestions" element={<AISuggestionsScreen />} />
        <Route path="/request-tracking" element={<RequestTrackingScreen />} />
      </Route>
    </Routes>
  )
}

export default App
