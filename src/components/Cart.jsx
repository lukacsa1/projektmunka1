const Cart = ({ cart, removeFromCart, setShowCart }) => {
    const formatPrice = (price) => `${price.toLocaleString()} Ft`;
    const totalAmount = cart.reduce((sum, item) => sum + item.product.ar * item.quantity, 0);
  
    return (
      <div className="cart-modal">
        <div className="cart-modal-content">
          <span className="close-cart" onClick={() => setShowCart(false)}>×</span>
          <h2>Kosár</h2>
          {cart.length === 0 ? (
            <p>A kosár üres.</p>
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
                    <button className="remove-btn" onClick={() => removeFromCart(item.product.id, item.size)}>
                      ❌
                    </button>
                  </div>
                ))}
              </div>
              <div className="cart-total">
                <p>Összesen: {formatPrice(totalAmount)}</p>
                <button className="checkout-btn" onClick={() => alert("Vásárlás!")}>
                  Pénztárhoz
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    );
  };
  
  export default Cart; // Alapértelmezett export hozzáadása
