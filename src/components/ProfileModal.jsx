import React, { useState, useEffect } from "react";
import axios from "axios";
import "../ProfilePage.css"; // Külső CSS fájl

// Definiáljuk a fetchProductDetails függvényt a ProfileModal komponensben:
const fetchProductDetails = async (productId) => {
  try {
    const response = await axios.get(`https://localhost:7117/api/Products/GetById/${productId}`);
    return response.data;
  } catch (error) {
    console.error("Termék lekérése sikertelen:", error);
    return null;
  }
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

  const handlePasswordChange = () => {
    if (newPassword !== confirmPassword) {
      alert("A jelszavak nem egyeznek!");
      return;
    }

    const token = localStorage.getItem("token")?.replace(/"/g, "");
    axios
      .post("https://localhost:7117/api/User/ChangePassword", {
        token,
        newPassword,
      })
      .then(() => alert("Jelszó sikeresen megváltoztatva!"))
      .catch(() => alert("Hiba történt a jelszó megváltoztatásakor."));
  };

  const toggleOrderDetails = (orderId) => {
    setExpandedOrderId(expandedOrderId === orderId ? null : orderId); // Ha már kinyitva, bezárja
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
                <input type="text" name="lastName" value={userData.lastName || ""} readOnly />
              </label>
              <label>
                <h4>Keresztnév:</h4>
                <input type="text" name="firstName" value={userData.firstName || ""} readOnly />
              </label>
              <label>
                <h4>Telefonszám:</h4>
                <input type="text" name="phoneNumber" value={userData.phoneNumber || ""} readOnly />
              </label>
              <label>
                <h4>E-mail:</h4>
                <input type="email" name="email" value={userData.email || ""} readOnly />
              </label>
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
                        }) || <p>Nincsenek termékek.</p>}
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

const ProductDetails = ({ productId }) => {
  const [product, setProduct] = useState(null);

  useEffect(() => {
    const fetchDetails = async () => {
      const details = await fetchProductDetails(productId);
      setProduct(details);
    };
    fetchDetails();
  }, [productId]);

  if (!product) return <p>Termék betöltése...</p>;

  // A 'meret' mező feldolgozása
  const sizes = JSON.parse(product.meret); // JSON.parse-t használunk a tömb feldolgozásához

  return (
    <div className="product-details">
      <p><strong>Termék neve:</strong> {product.termekNeve}</p>

      <p><strong>Ár:</strong> {product.ar} Ft</p>
      <p><strong>Kategória:</strong> {product.kategoria}</p>

    
    </div>
  );
};

export default ProfileModal;
