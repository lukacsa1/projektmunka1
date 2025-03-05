import React, { useState, useEffect } from "react";
import axios from "axios";
import "../ProfilePage.css"; // Külső CSS fájl

const ProfileModal = ({ setShowProfile }) => {
  const [userData, setUserData] = useState({
    lastName: "",
    firstName: "",
    phoneNumber: "",
    email: "",
    Orders: [], // Adding the Orders array here
    loginName: "", // Login name for the user
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeSection, setActiveSection] = useState("data");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  useEffect(() => {
    const token = localStorage.getItem("token")?.replace(/"/g, "");
    if (token) {
      axios
        .get(`https://localhost:7117/api/User/GetByToken?token=${token}`)
        .then((response) => {
          setUserData(response.data);
          setLoading(false);
        })
        .catch(() => {
          setError("Nem sikerült betölteni az adatokat.");
          setLoading(false);
        });
    } else {
      setError("Nincs érvényes bejelentkezési token.");
      setLoading(false);
    }
  }, []);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setUserData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  const handleSave = () => {
    const token = localStorage.getItem("token")?.replace(/"/g, "");
    if (token) {
      axios
        .put(
          `https://localhost:7117/api/User/UpdateUserDetails?token=${token}`,
          {
            loginName: userData.loginName, // Az eredeti felhasználónév
            firstName: userData.firstName,
            lastName: userData.lastName,
            phoneNumber: userData.phoneNumber,
          },
          {
            headers: {
              'Content-Type': 'application/json',
              'accept': '*/*',
            },
          }
        )
        .then((response) => {
          alert("Adatok sikeresen mentve!");
        })
        .catch((error) => {
          alert("Hiba történt az adatok mentésekor.");
          console.error(error);
        });
    } else {
      alert("Nincs érvényes token.");
    }
  };

  const hashPassword = async (password, salt) => {
    const encoder = new TextEncoder();
    const data = encoder.encode(password + salt);
    const hashBuffer = await crypto.subtle.digest("SHA-256", data); // Hashing
    const hashArray = Array.from(new Uint8Array(hashBuffer)); // Array from buffer
    return hashArray.map(byte => byte.toString(16).padStart(2, '0')).join(''); // Hex formátum
  };

  const handleChangePassword = async () => {
    if (newPassword !== confirmPassword) {
      alert("A két jelszó nem egyezik.");
      return;
    }

    const salt = "a_random_salt"; // Használj egy erős sót a titkosításhoz
    const hashedPassword = await hashPassword(newPassword, salt); // Jelszó hash-elése

    const token = localStorage.getItem("token")?.replace(/"/g, "");
    if (token) {
      axios
        .post(
          `https://localhost:7117/api/User/RequestChangePassword?token=${token}`,
          { newPassword: hashedPassword }, // A hash-elt jelszó küldése
          {
            headers: {
              'Content-Type': 'application/json',
              'accept': '*/*',
            },
          }
        )
        .then((response) => {
          alert("Jelszó sikeresen megváltoztatva!");
        })
        .catch((error) => {
          alert("Hiba történt a jelszó változtatásakor.");
          console.error(error);
        });
    } else {
      alert("Nincs érvényes token.");
    }
  };

  const handleClose = () => {
    setShowProfile(false); // Bezárja a profilt
  };

  if (loading) return <div>Betöltés...</div>;
  if (error) return <div>{error}</div>;

  return (
    <div className="profile-modal">
      {/* Oldalsáv */}
      <div className="sidebar">
        <h3>Szia <br /> {userData.loginName}!</h3>
        <ul>
          <li onClick={() => setActiveSection("data")} className={activeSection === "data" ? "active" : ""}>
            Adataim
          </li>
          <li onClick={() => setActiveSection("orders")} className={activeSection === "orders" ? "active" : ""}>
            Vásárlásaim
          </li>
          <li onClick={() => setActiveSection("change-password")} className={activeSection === "change-password" ? "active" : ""}>
            Jelszó megváltoztatása
          </li>
          <li onClick={handleClose}>Bezárás</li> {/* Bezárás gomb */}
        </ul>
      </div>

      {/* Tartalom */}
      <div className="content">
        {activeSection === "data" && (
          <>
            <h2>Személyes adataim</h2>
            <form >
              <label >
              <h4>Vezetéknév:</h4>  
                <input type="text" name="lastName" value={userData.lastName} onChange={handleInputChange} />
              </label>
              <label>
              <h4>  Keresztnév:</h4>
                <input type="text" name="firstName" value={userData.firstName} onChange={handleInputChange} />
              </label>
              <label>
              <h4> Telefonszám:</h4>
                <input type="text" name="phoneNumber" value={userData.phoneNumber} onChange={handleInputChange} />
              </label>
              <label>
              <h4> E-mail:</h4>
                <input type="email" name="email" value={userData.email} onChange={handleInputChange} />
              </label>
              {/* Mentés gomb */}
      {activeSection === "data" && (
        <button className="save-button" onClick={handleSave}>
          Adatok mentése
        </button>
      )}
            </form>
          </>
        )}

        {/* Rendelések szekció */}
        {activeSection === "orders" && (
          <div className="section">
            <h3>Előző rendeléseim</h3>
            {userData.Orders && userData.Orders.length > 0 ? (
              userData.Orders.map((order) => (
                <div key={order.id}>
                  <p><strong>Rendelés ID:</strong> {order.id}</p>
                  <p><strong>Dátum:</strong> {new Date(order.date).toLocaleDateString()}</p>
                  <p><strong>Összeg:</strong> {order.totalPrice} Ft</p>
                  <button onClick={() => alert(`Rendelés részletei: ${order.id}`)}>
                    Rendelés részletei
                  </button>
                </div>
              ))
            ) : (
              <p>Még nem rendeltek.</p>
            )}
          </div>
        )}

        {/* Jelszó változtatása */}
        {activeSection === "change-password" && (
          <div className="password-change-section">
            <h3 className="password-change-title">Jelszó megváltoztatása</h3>
            <label className="password-label">
              Új jelszó:
              <input 
                type="password" 
                value={newPassword} 
                onChange={(e) => setNewPassword(e.target.value)} 
                className="password-input" 
              />
            </label>
            <label className="password-label">
              Új jelszó megerősítése:
              <input 
                type="password" 
                value={confirmPassword} 
                onChange={(e) => setConfirmPassword(e.target.value)} 
                className="password-input" 
              />
            </label>
            <button className="password-change-button" onClick={handleChangePassword}>Jelszó változtatása</button>
          </div>
        )}
      </div>

      
    </div>
  );
};

export default ProfileModal;