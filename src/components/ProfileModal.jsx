import React, { useState, useEffect } from "react";
import axios from "axios";

const ProfileModal = ({ setShowProfile }) => {
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeSection, setActiveSection] = useState("data");

  useEffect(() => {
    const token = localStorage.getItem("token").replace(/"/g, "");
    if (token) {
      axios
        .get(`https://localhost:7117/api/User/GetByToken?token=${token}`)
        .then((response) => {
          setUserData(response.data);
          setLoading(false);
        })
        .catch((error) => {
          setError("Nem sikerült betölteni az adatokat.");
          setLoading(false);
        });
    } else {
      setError("Nincs érvényes bejelentkezési token.");
      setLoading(false);
    }
  }, []);

  const handleSectionChange = (section) => {
    setActiveSection(section);
  };

  if (loading) {
    return <div>Betöltés...</div>;
  }

  if (error) {
    return <div>{error}</div>;
  }

  return (
    <div className="profile-modal">
      <h2>Profil</h2>
      <div className="profile-buttons">
        <button onClick={() => handleSectionChange("data")}>Adataim</button>
        <button onClick={() => handleSectionChange("orders")}>Előző rendeléseim</button>
        <button onClick={() => handleSectionChange("password")}>Jelszó módosítása</button>
        <button onClick={() => handleSectionChange("address")}>Számlázási cím módosítása</button>
      </div>

      {/* Adataim szekció */}
      {activeSection === "data" && (
        <div className="section">
          <p><strong>Felhasználó neve:</strong> {userData.loginName}</p>
          <p><strong>Email:</strong> {userData.email}</p>
        </div>
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

      {/* Jelszó módosítása szekció */}
      {activeSection === "password" && (
        <div className="section">
          <p>Jelszó módosítása</p>
          <button onClick={() => alert("Jelszó módosítása")} >
            Jelszó megváltoztatása
          </button>
        </div>
      )}

      {/* Számlázási cím módosítása szekció */}
      {activeSection === "address" && (
        <div className="section">
          <p>Számlázási cím módosítása</p>
          <button onClick={() => alert("Cím módosítása")}>
            Szállítási cím módosítása
          </button>
        </div>
      )}

      {/* Bezárás gomb */}
      <button onClick={() => setShowProfile(false)} style={{ marginTop: "10px" }}>
        Bezárás
      </button>
    </div>
  );
};

export default ProfileModal;
