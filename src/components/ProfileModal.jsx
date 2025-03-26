import React, { useState, useEffect } from "react";
import axios from "axios";
import "../ProfilePage.css"; // Külső CSS fájl








// Hashelési funkció például SHA256
const hashPassword = async (password, salt) => {
  const encoder = new TextEncoder();
  const data = encoder.encode(password + salt);
  const hashBuffer = await crypto.subtle.digest("SHA-256", data); // Hashing
  const hashArray = Array.from(new Uint8Array(hashBuffer)); // Array from buffer
  return hashArray.map(byte => byte.toString(16).padStart(2, '0')).join(''); // Hex formátum
};
// Termék részletező komponens importálása
const ProductDetails = ({ productId }) => {
  const [product, setProduct] = useState(null);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        const response = await axios.get(`https://localhost:7117/api/Products/GetById/${productId}`);
        setProduct(response.data);
      } catch (error) {
        console.error("Termék részletei nem tölthetők be:", error);
      }
    };

    fetchProduct();
  }, [productId]);

  if (!product) return <div>Termék betöltése...</div>;

  return (
    <div>
      <h4>{product.termekNeve}</h4>
      <p>{product.description}</p>
      <p>Ár: {product.ar} Ft</p>
    </div>
  );
};

const ProfileModal = ({ setShowProfile }) => {
  const [userData, setUserData] = useState({
    lastName: "",
    firstName: "",
    phoneNumber: "",
    email: "",
    loginName: "",
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeSection, setActiveSection] = useState("data");
  const [newPassword, setNewPassword] = useState("");
  const [oldpassword, setoldpassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [orders, setOrders] = useState([]);
  const [ordersLoading, setOrdersLoading] = useState(false);
  const [ordersError, setOrdersError] = useState(null);
  const [expandedOrderId, setExpandedOrderId] = useState(null); // Állapot a rendelés kibővítéséhez
  

  useEffect(() => {
    const token = localStorage.getItem("token")?.replace(/"/g, "");
    if (token) {
      axios
        .get(`https://localhost:7117/api/User/GetByToken?token=${token}`)
        .then((response) => {
          setUserData(response.data || {});
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

  useEffect(() => {
    if (activeSection === "orders") {
      fetchOrders();
    }
  }, [activeSection]);

  const fetchOrders = () => {
    const token = localStorage.getItem("token")?.replace(/"/g, "");
    if (!token) {
      setOrdersError("Nincs érvényes token.");
      return;
    }

    setOrdersLoading(true);
    axios
      .get(`https://localhost:7117/api/Order/GetOrdersByUser?token=${token}`)
      .then((response) => {
        setOrders(response.data || []);
        setOrdersLoading(false);
      })
      .catch(() => {
        setOrdersError("Nem sikerült lekérni a rendeléseket.");
        setOrdersLoading(false);
      });
  };

  const handleClose = () => {
    setShowProfile(false);
  };

   const handlePasswordChange = async () => {
    if (newPassword !== confirmPassword) {
      alert("A jelszavak nem egyeznek!");
      return;
    }
    const newSalt= await fetch(`https://localhost:7117/api/Registration/GetNewSalt`, {
      method: "GET",
      headers: {
          "Content-Type": "application/json",
      },
    });
    const oldSalt= fetch(`https://localhost:7117/api/Login/GetSalt/${ userData.loginName}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });
    if (!oldSalt || oldSalt === 'null') {
      throw new Error("A só nem érvényes vagy üres.");
    }

    const salt = await newSalt.text();

const newPasswordHash = await hashPassword(newPassword, salt); // Hashelés a jelszóval és sóval
const oldpasswordhash=await hashPassword(oldpassword, userData.salt);
    const token = localStorage.getItem("token")?.replace(/"/g, "");
    axios
      .put(`https://localhost:7117/api/User/ChangePassword?token=${token}`, {
        oldPasswordHash:oldpasswordhash,
        newPasswordHash:newPasswordHash,
        newSalt: salt,
      })
      .then(() => alert("Jelszó sikeresen megváltoztatva!"))
      .catch(() => alert("Hiba történt a jelszó megváltoztatásakor."));
     
      
  };

  const toggleOrderDetails = (orderId) => {
    setExpandedOrderId(expandedOrderId === orderId ? null : orderId); // Ha már kinyitva, bezárja
  };

  const handleSaveChanges = () => {
    const token = localStorage.getItem("token")?.replace(/"/g, "");
    if (!token) {
      alert("Nincs érvényes token.");
      return;
    }

    axios
      .put(`https://localhost:7117/api/User/UpdateUserDetails?token=${token}`, {
        token,
        loginName: userData.loginName,
        firstName: userData.firstName,
        lastName: userData.lastName,
        phoneNumber: userData.phoneNumber,
      })
      .then(() => alert("A profil sikeresen frissítve!"))
      .catch(() => alert("Hiba történt a profil frissítésekor."));
    
  };
  

  if (loading) return <div>Betöltés...</div>;
  if (error) return <div>{error}</div>;

  return (
    <div className="profile-modal">
      <div className="sidebar">
        <h3>Szia <br /> {userData.loginName || "Felhasználó"}!</h3>
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
          <li onClick={handleClose}>Bezárás</li>
        </ul>
      </div>

      <div className="content" style={{ color: "white" }}>
        {activeSection === "data" && (
          <>
            <h2>Személyes adataim</h2>
            <form>
              <label>
                <h4>Vezetéknév:</h4>
                <input
                  type="text"
                  name="lastName"
                  value={userData.lastName || ""}
                  onChange={(e) => setUserData({ ...userData, lastName: e.target.value })}
                />
              </label>
              <label>
                <h4>Keresztnév:</h4>
                <input
                  type="text"
                  name="firstName"
                  value={userData.firstName || ""}
                  onChange={(e) => setUserData({ ...userData, firstName: e.target.value })}
                />
              </label>
              <label>
                <h4>Telefonszám:</h4>
                <input
                  type="text"
                  name="phoneNumber"
                  value={userData.phoneNumber || ""}
                  onChange={(e) => setUserData({ ...userData, phoneNumber: e.target.value })}
                />
              </label>
              <label>
                <h4>E-mail:</h4>
                <input type="email" name="email" value={userData.email || ""} readOnly />
              </label>
              <button type="button" onClick={handleSaveChanges}>Módosítás mentése</button>
            </form>
          </>
        )}

        {activeSection === "orders" && (
          <div className="section">
            <h3>Előző rendeléseim</h3>
            {ordersLoading && <p>Betöltés...</p>}
            {ordersError && <p>{ordersError}</p>}
            {!ordersLoading && !ordersError && orders.length === 0 && <p>Még nem rendeltek.</p>}
            {!ordersLoading && orders.map((order) => {
              const formattedDate = order.datum ? new Date(order.datum).toLocaleDateString() : "Ismeretlen dátum";

              return (
                <div key={order.id} className="order-card">
                  <p><strong>Dátum:</strong> {formattedDate}</p>
                  <p><strong>Rendelési szám:</strong> {order.orderNumber}</p>
                  <p><strong>Rendelési státusz:</strong>
                    {order.status === 0 && "Összekészítés alatt"}
                    {order.status === 1 && "Kézbesítés alatt"}
                    {order.status === 2 && "Kézbesítve"}
                  </p>
                  <button onClick={() => toggleOrderDetails(order.id)}>
                    {expandedOrderId === order.id ? "Részletek elrejtése" : "Részletek megtekintése"}
                  </button>
                  {expandedOrderId === order.id && (
                    <div className="order-details">
                      <ul>
                        {order.orderitems?.map((orderItem, index) => {
                          if (!orderItem.termekId) return null;
                          return (
                            <li key={index}>
                              <ProductDetails productId={orderItem.termekId} />
                              <p>Méret: {orderItem.meret}</p>
                              <p>Darabszám: {orderItem.darabszam}</p>
                            </li>
                          );
                        }) || <p>Nincsenek termékek a rendeléshez.</p>}
                      </ul>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}

        {activeSection === "change-password" && (
          <div className="password-change-section">
            <h3 className="password-change-title">Jelszó megváltoztatása</h3>
            <label className="password-label">
              Régi jelszó:
              <input
                type="password"
                value={oldpassword}
                onChange={(e) => setoldpassword(e.target.value)}
                className="password-input"
              />
            </label>

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
            <button onClick={handlePasswordChange} className="password-change-button">
              Jelszó változtatása
            </button>
          </div>
        )}
      </div>

    </div>
    
  );
  
};

export default ProfileModal;
