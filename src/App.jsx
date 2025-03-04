import React, { useState, useEffect } from "react";
import "./App.css";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

// Komponensek importálása
import Navbar from "./components/Navbar";
import ProductGrid from "./components/ProductGrid";
import Cart from "./components/Cart";
import LoginModal from "./components/LoginModal";
import RegistrationModal from "./components/RegistrationModal";
import LogoutModal from "./components/LogoutModal";
import ProfileModal from "./components/ProfileModal";

const App = () => {
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [category, setCategory] = useState("none");
  const [selectedCategory, setSelectedCategory] = useState("Ajánlott termékek");
  const [searchQuery, setSearchQuery] = useState("");
  const [showCart, setShowCart] = useState(false);
  const [showLogin, setShowLogin] = useState(false);
  const [showRegistration, setShowRegistration] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [showLogout, setShowLogout] = useState(false);
  const [showProfile, setShowProfile] = useState(false);

  useEffect(() => {
    fetch("https://localhost:7117/api/Products/GetProducts")
      .then((response) => response.json())
      .then((data) => setProducts(data))
      .catch((error) => console.error("Hiba a fetch kérés során:", error));
  }, []);

  const addToCart = (product, size, quantity) => {
    setCart((prevCart) => {
      const existingItem = prevCart.find(
        (item) => item.product.id === product.id && item.size === size
      );
      if (existingItem) {
        return prevCart.map((item) =>
          item.product.id === product.id && item.size === size
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
      }
      return [...prevCart, { product, size, quantity }];
    });
  };

  const removeFromCart = (productId, size) => {
    setCart((prevCart) =>
      prevCart.filter((item) => !(item.product.id === productId && item.size === size))
    );
  };

  const handleLoginSuccess = () => {
    setIsLoggedIn(true);
    setShowLogin(false);
  };

  const handleLogoutSuccess = () => {
    setIsLoggedIn(false);
    setShowLogout(false);
  };

  return (
    <Router>
      <Navbar
        cartSize={cart.length}
        setCategory={setCategory}
        setSearchQuery={setSearchQuery}
        searchQuery={searchQuery}
        setShowCart={setShowCart}
        setShowLogin={setShowLogin}
        isLoggedIn={isLoggedIn}
        setShowLogout={setShowLogout}
        setIsLoggedIn={setIsLoggedIn}
        setShowProfile={setShowProfile}
      />
      <Routes>
        <Route
          path="/"
          element={
            <>
              <section className="banner">
                <h2>Fedezd fel a legújabb pólóinkat!</h2>
                <p>Válassz a legfrissebb trendek közül és vásárolj online.</p>
              </section>
              <ProductGrid
                products={products}
                category={category}
                selectedCategory={selectedCategory}
                searchQuery={searchQuery}
                addToCart={addToCart}
              />
            </>
          }
        />
      </Routes>
      {showCart && <Cart cart={cart} removeFromCart={removeFromCart} setShowCart={setShowCart} />}
      {showLogin && (
        <LoginModal
          setShowLogin={setShowLogin}
          setShowRegistration={setShowRegistration}
          onLoginSuccess={handleLoginSuccess}
        />
      )}
      {showRegistration && <RegistrationModal setShowRegistration={setShowRegistration} />}
      {showLogout && <LogoutModal setShowLogout={setShowLogout} onLogoutSuccess={handleLogoutSuccess} />}
      {showProfile && <ProfileModal setShowProfile={setShowProfile} />}
    </Router>
  );
};

export default App;
