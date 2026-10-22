import { useState } from "react";

function CreateRequestScreen() {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [location, setLocation] = useState("");

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
  }

  return (
    <div dir="rtl">
      <h1>בקשת עזרה חדשה</h1>
      <p>תארי את הבעיה כדי שנוכל למצוא עבורך עזרה מתאימה</p>

      <form onSubmit={handleSubmit}>
        <div>
          <label>כותרת הבקשה</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="לדוגמה: תקר בגלגל"
          />
        </div>

        <div>
          <label>תיאור הבעיה</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="תארי מה קרה ואיזו עזרה נדרשת"
          />
        </div>

        

        <div>
          <label>מיקום</label>
          <input
            type="text"
            value={location}
            onChange={(e) => setLocation(e.target.value)}
            placeholder="הזיני כתובת או אזור"
          />
        </div>

        <button type="submit">
          הצגת הבקשה
        </button>
      </form>
    </div>
  );
}

export default CreateRequestScreen;