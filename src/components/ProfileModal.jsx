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
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeSection, setActiveSection] = useState("data");

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
    alert("Adatok mentése...");
    // Itt lehetne egy PUT kérés az adatok mentéséhez
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
          <li onClick={handleClose}>Bezárás</li> {/* Bezárás gomb */}
        </ul>
      </div>

      {/* Tartalom */}
      <div className="content">
        {activeSection === "data" && (
          <>
            <h2>Személyes adataim</h2>
            <form>
              <label>
                Vezetéknév:
                <input type="text" name="lastName" value={userData.lastName} onChange={handleInputChange} />
              </label>
              <label>
                Keresztnév:
                <input type="text" name="firstName" value={userData.firstName} onChange={handleInputChange} />
              </label>
              <label>
                Telefonszám:
                <input type="text" name="phoneNumber" value={userData.phoneNumber} onChange={handleInputChange} />
              </label>
              <label>
                E-mail:
                <input type="email" name="email" value={userData.email} onChange={handleInputChange} />
              </label>
            </form>
            <p><a href="#change-password">Jelszó megváltoztatása</a></p>
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
      </div>

      {/* Mentés gomb */}
      {activeSection === "data" && (
        <button className="save-button" onClick={handleSave}>
          Adatok mentése
        </button>
      )}
    </div>
  );
};

export default ProfileModal;
