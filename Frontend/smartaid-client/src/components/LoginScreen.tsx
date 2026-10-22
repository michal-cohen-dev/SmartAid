import { useState } from "react";

function LoginScreen() {
  const [emailOrPhone, setEmailOrPhone] = useState("");
  const [password, setPassword] = useState("");

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
  
  }

  return (
    <div dir="rtl">
      <div>
        <h1>ברוכים הבאים ל־SmartAid</h1>
        <p>עזרה חכמה. קהילה חזקה.</p>

        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="loginEmail">אימייל או טלפון</label>
            <input
              id="loginEmail"
              type="text"
              value={emailOrPhone}
              onChange={(event) => setEmailOrPhone(event.target.value)}
              placeholder="הזיני אימייל או מספר טלפון"
            />
          </div>

          <div>
            <label htmlFor="loginPassword">סיסמה</label>
            <input
              id="loginPassword"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="הזיני סיסמה"
            />
          </div>

          <button type="submit">התחברות</button>
        </form>

        <p>עדיין אין לך חשבון? הירשמי עכשיו</p>
      </div>
    </div>
  );
}

export default LoginScreen;