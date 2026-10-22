import { useState } from "react";

function RegistrationScreen() {
  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    
  }

  return (
    <div dir="rtl">
      <div>
        <h1>יצירת חשבון</h1>
        <p>הצטרפי לקהילת SmartAid</p>

        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="fullName">שם מלא</label>
            <input
              id="fullName"
              type="text"
              value={fullName}
              onChange={(event) => setFullName(event.target.value)}
              placeholder="הזיני שם מלא"
            />
          </div>

          <div>
            <label htmlFor="phone">טלפון</label>
            <input
              id="phone"
              type="tel"
              value={phone}
              onChange={(event) => setPhone(event.target.value)}
              placeholder="הזיני מספר טלפון"
            />
          </div>

          <div>
            <label htmlFor="email">אימייל</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="name@example.com"
            />
          </div>

          <div>
            <label htmlFor="password">סיסמה</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="בחרי סיסמה"
            />
          </div>

          <div>
            <label htmlFor="confirmPassword">אישור סיסמה</label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              placeholder="הקלידי שוב את הסיסמה"
            />
          </div>

          
          <button type="submit">הרשמה</button>
        </form>

        <p>כבר יש לך חשבון? התחברי עכשיו</p>
      </div>
    </div>
  );
}

export default RegistrationScreen;