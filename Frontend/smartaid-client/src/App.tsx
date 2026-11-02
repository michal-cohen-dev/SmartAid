import './App.css'
import { Routes, Route } from 'react-router-dom'
import RequestTrackingScreen from './components/RequestTrackingScreen'
import MainDashboard from './components/MainDashboard'
import LoginScreen from './components/LoginScreen'
import RegistrationScreen from './components/RegistrationScreen'
import CreateRequestScreen from './components/CreateRequestScreen'
import AISuggestionsScreen from './components/AISuggestionsScreen'
import RequestDetailsScreen from './components/RequestDetailsScreen'

function App() {
  return (
    <Routes>
      <Route path="/" element={<MainDashboard />} />
      <Route path="/login" element={<LoginScreen />} />
      <Route path="/register" element={<RegistrationScreen />} />
      <Route path="/request" element={<CreateRequestScreen />} />
      <Route path="/suggestions" element={<AISuggestionsScreen />} />
      <Route path="/request-details" element={<RequestDetailsScreen />} />
      <Route path="/request-tracking" element={<RequestTrackingScreen />}
/>
    </Routes>
  )
}

export default App