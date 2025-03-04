import React, { useState } from "react";

// Hashelési funkció SHA256
const hashPassword = async (password, salt) => {
  const encoder = new TextEncoder();
  const data = encoder.encode(password + salt);
  const hashBuffer = await crypto.subtle.digest("SHA-256", data); // Hashing
  const hashArray = Array.from(new Uint8Array(hashBuffer)); // Array from buffer
  return hashArray.map(byte => byte.toString(16).padStart(2, '0')).join(''); // Hex formátum
};

const LoginModal = ({ setShowLogin, setShowRegistration, onLoginSuccess, token }) => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(""); // Reseteljük az esetleges előző hibát

    if (!username || !password) {
      setError("Felhasználónév és jelszó megadása kötelező!");
      return;
    }

    try {
      // Először lekérjük a sót a `GetSalt` végpontról
      const saltResponse = await fetch(`https://localhost:7117/api/Login/GetSalt/${username}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      });

      const salt = await saltResponse.text(); // A válasz szöveges formában

      // Ha a só üres, hibát dobunk
      if (!salt || salt === 'null') {
        throw new Error("A só nem érvényes vagy üres.");
      }

      // A jelszó és a só hashelése
      const tmpHash = await hashPassword(password, salt);

      // Most küldjük el a helyes formátumban
      const loginResponse = await fetch("https://localhost:7117/api/Login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ loginName: username, tmpHash }), // A hashelt jelszó és a felhasználónév
      });

      const loginTextData = await loginResponse.text();

      // Ellenőrizzük, hogy a válasz JSON-e
      let data = {};
      try {
        data = JSON.parse(loginTextData); // Próbáljuk JSON-ként értelmezni
      } catch (err) {
        throw new Error("A válasz nem JSON formátumban érkezett!" + loginResponse.status + " " + loginTextData);
      }

      if (!loginResponse.ok) {
        throw new Error(data.message || "Hibás bejelentkezési adatok!");
      }

      // Sikeres bejelentkezés esetén elmentjük a tokent.
      localStorage.setItem("token", JSON.stringify(data.token));

      setShowLogin(false);
      onLoginSuccess();
      alert("Sikeres bejelentkezés!");
    } catch (err) {
      setError(err.message); // A hibát jelenítjük meg
    }
  };

  return (
    <div className="login-modal">
      <div className="login-modal-content">
        <span className="close-login" onClick={() => setShowLogin(false)}>×</span>
        <h2>Bejelentkezés</h2>
        {error && <p style={{ color: "red" }}>{error}</p>}
        <form onSubmit={handleSubmit}>
          <label>Felhasználónév:</label>
          <input
            type="text"
            placeholder="Felhasználónév"
            required
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />

          <label>Jelszó:</label>
          <input
            type="password"
            placeholder="Jelszó"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          <button type="submit">Belépés</button>
          <a href="#" onClick={() => {
            setShowRegistration(true);
            setShowLogin(false);
          }}>Regisztrálj</a>
        </form>
      </div>
    </div>
  );
};

export default LoginModal;
