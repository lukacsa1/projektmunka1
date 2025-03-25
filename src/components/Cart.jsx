import { useState, useEffect } from "react";
import axios from "axios";

const Cart = ({ cart, removeFromCart, setShowCart }) => {
  const [checkout, setCheckout] = useState(false);
  const [billingInfo, setBillingInfo] = useState({ name: "", email: "", phone: "", address: "" });
  const [useSavedBilling, setUseSavedBilling] = useState(false);
  const [showSavePrompt, setShowSavePrompt] = useState(false);

  useEffect(() => {
    // Ha a checkbox be van pipálva, lekérjük a mentett számlázási adatokat
    if (useSavedBilling) {
      axios.get("http://localhost:3000/api/user/billing-info")
        .then(response => {
          // A válaszban kapott adatokat beállítjuk
          setBillingInfo(response.data);
        })
        .catch(error => {
          console.error("Hiba a mentett számlázási adatok betöltésekor:", error);
        });
    } else {
      // Ha nincs bepipálva, töröljük a számlázási adatokat
      setBillingInfo({ name: "", email: "", phone: "", address: "" });
    }
  }, [useSavedBilling]);

  const formatPrice = (price) => `${price.toLocaleString()} Ft`;
  const totalAmount = cart.reduce((sum, item) => sum + item.product.ar * item.quantity, 0);

  const handleChange = (e) => {
    setBillingInfo({ ...billingInfo, [e.target.name]: e.target.value });
  };

  const handleCheckout = () => {
    setCheckout(true);
  };

  const handleConfirmOrder = async () => {
    const orderData = {
      customer: billingInfo,
      items: cart.map(item => ({
        productId: item.product.id,
        productName: item.product.termekNeve,
        size: item.size,
        quantity: item.quantity,
        price: item.product.ar,
      })),
      total: totalAmount,
    };

    try {
      await axios.post("http://localhost:3000/api/orders", orderData);
      alert("Rendelés sikeresen elküldve!");
      setShowSavePrompt(true);
    } catch (error) {
      console.error("Hiba történt a rendelés leadásakor:", error);
      alert("Hiba történt a rendelés elküldése közben.");
    }
  };

  const handleSaveBillingInfo = async () => {
    try {
      await axios.post("http://localhost:3000/api/user/save-billing-info", billingInfo);
      alert("Számlázási adatok sikeresen mentve!");
      setShowSavePrompt(false);
      setShowCart(false);
    } catch (error) {
      console.error("Hiba történt a számlázási adatok mentésekor:", error);
      alert("Hiba történt a számlázási adatok mentése közben.");
    }
  };

  return (
    <div className="cart-modal">
      <div className="cart-modal-content">
        <span className="close-cart" onClick={() => setShowCart(false)}>×</span>
        <h2>{checkout ? "Számlázási adatok" : "Kosár"}</h2>

        {checkout ? (
          <div className="checkout-container">
            <div className="cart-summary">
              <div className="cart-items">
                {cart.map((item, index) => (
                  <div key={index} className="cart-item">
                    <img src={`https://localhost:7117/api/Image/ProductImages/GetImageByName/${item.product.kep}`} alt={item.product.termekNeve} />
                    <div className="cart-item-details">
                      <p>{item.product.termekNeve}</p>
                      <p>Méret: {item.size}</p>
                      <p>Mennyiség: {item.quantity}</p>
                      <p>Ár: {formatPrice(item.product.ar)}</p>
                    </div>
                  </div>
                ))}
              </div>
              <p className="total-price">Összesen: {formatPrice(totalAmount)}</p>
            </div>

            <div className="billing-form">
              <label>Név:</label>
              <input type="text" name="name" value={billingInfo.name} onChange={handleChange} required />

              <label>Email:</label>
              <input type="email" name="email" value={billingInfo.email} onChange={handleChange} required />

              <label>Telefon:</label>
              <input type="tel" name="phone" value={billingInfo.phone} onChange={handleChange} required />

              <label>Cím:</label>
              <input type="text" name="address" value={billingInfo.address} onChange={handleChange} required />
              <label>
              <input className="szamlazasicheckbox" type="checkbox" checked={useSavedBilling} onChange={() => setUseSavedBilling(!useSavedBilling)}  /> Mentett számlázási adatok használata 
             
              </label>
              <button className="confirm-btn" onClick={handleConfirmOrder}>Rendelés leadása</button>
            </div>
          </div>
        ) : (
          <>
            <div className="cart-items">
              {cart.map((item, index) => (
                <div key={index} className="cart-item">
                  <img src={`https://localhost:7117/api/Image/ProductImages/GetImageByName/${item.product.kep}`} alt={item.product.termekNeve} />
                  <div className="cart-item-details">
                    <p>{item.product.termekNeve}</p>
                    <p>Ár: {formatPrice(item.product.ar)}</p>
                    <p>Méret: {item.size}</p>
                    <p>Mennyiség: {item.quantity}</p>
                  </div>
                  <button className="remove-btn" onClick={() => removeFromCart(item.product.id, item.size)}>❌</button>
                </div>
              ))}
            </div>
            <div className="cart-total">
              <p>Összesen: {formatPrice(totalAmount)}</p>
              <button className="checkout-btn" onClick={handleCheckout}>Pénztárhoz</button>
            </div>
          </>
        )}

        {showSavePrompt && (
          <div className="save-prompt">
            <p>Szeretné menteni a számlázási adatait?</p>
            <button onClick={handleSaveBillingInfo}>Igen</button>
            <button onClick={() => setShowSavePrompt(false)}>Nem</button>
          </div>
        )}
      </div>
    </div>
  );
};

export default Cart;
