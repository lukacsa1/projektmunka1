import { useState } from "react";
import axios from "axios";


const Cart = ({ cart, removeFromCart, setShowCart }) => {
  const [checkout, setCheckout] = useState(false);
  const [billingInfo, setBillingInfo] = useState({
    name: "",
    email: "",
    phoneNumber: "",
    postalCode: "",
    country: "",
    city: "",
    street: "",
    houseNumber: ""
  });

  const formatPrice = (price) => `${price.toLocaleString()} Ft`;
  const totalAmount = cart.reduce((sum, item) => sum + item.product.ar * item.quantity, 0);

  const handleChange = (e) => {
    const { name, value } = e.target;
    if (name === "postalCode" && !/^[0-9]*$/.test(value)) {
      return;
    }
    setBillingInfo({ ...billingInfo, [name]: value });
  };

  const handleCheckout = () => {
    setCheckout(true);
  };

  const handleConfirmOrder = async () => {
    const orderData = {
      billing: { ...billingInfo },
      orderProducts: cart.map(item => ({
        id: item.product.id,
        size: item.size,
        amount: item.quantity
      }))
    };

    try {
      const token = localStorage.getItem("token")?.replace(/"/g, "");
      const response = await axios.post(`https://localhost:7117/api/Order/NewOrder?token=${token}`, orderData);
      alert("Rendelés sikeresen elküldve!");
      setShowCart(false);
    } catch (error) {
      if (error.response && error.response.status === 400) {
        const errorMessage = error.response.data;
        if (errorMessage === "Túl sok megrendelt termék!") {
          alert("A rendelésedben túl sok termék van. Kérlek csökkentsd a mennyiséget.");
        } else {
          alert("Hiba történt a rendelés elküldése közben.");
        }
      } else {
      
        console.error("Hiba történt a rendelés leadásakor:", error);
        alert("Hiba történt a rendelés elküldése közben.");
      }
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
              <input type="tel" name="phoneNumber" value={billingInfo.phoneNumber} onChange={handleChange} required />

              <label>Irányítószám:</label>
              <input type="text" name="postalCode" value={billingInfo.postalCode} onChange={handleChange} required />

              <label>Ország:</label>
              <input type="text" name="country" value={billingInfo.country} onChange={handleChange} required />

              <label>Város:</label>
              <input type="text" name="city" value={billingInfo.city} onChange={handleChange} required />

              <label>Utca:</label>
              <input type="text" name="street" value={billingInfo.street} onChange={handleChange} required />

              <label>Házszám:</label>
              <input type="text" name="houseNumber" value={billingInfo.houseNumber} onChange={handleChange} required />

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
      </div>
    </div>
  );
};

export default Cart;