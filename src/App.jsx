import React, { useState, useEffect } from "react";
import "./App.css";
import axios from "axios";

const App = () => {
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [category, setCategory] = useState("none");
  const [selectedCategory, setSelectedCategory] = useState("Ajánlott termékek");
  const [searchQuery, setSearchQuery] = useState("");
  const [showCart, setShowCart] = useState(false); // Kosár megjelenítése
  const [showLogin, setShowLogin] = useState(false); // Bejelentkezési modal megjelenítése
  const [showRegistration, setShowRegistration] = useState(false); // Bejelentkezési modal megjelenítése
  const [isLoggedIn, setIsLoggedIn] = useState(false); // Új állapot a bejelentkezéshez
  const [isLoggedOut, setIsLoggedOut] = useState(false); // Új állapot a kijelentkezéshez
  const [showLogout, setShowLogout] = useState(false);

  useEffect(() => {
    fetch("https://localhost:7117/Termekek/GetProducts")
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
    setCart((prevCart) => prevCart.filter((item) => !(item.product.id === productId && item.size === size)));
  };

  const handleCategoryChange = (e, categoryName, categoryTitle) => {
    e.preventDefault();
    setCategory(categoryName);
    setSelectedCategory(categoryTitle); // 🔹 Kategória címének frissítése
  };

  const handleLoginSuccess = () => {
    setIsLoggedIn(true); // Sikeres bejelentkezés után beállítjuk, hogy be van jelentkezve
    setShowLogin(false);  // Bezárjuk a bejelentkezési modalt
  };

  const handleLogoutSuccess = () => {
    setIsLoggedOut(true); // Sikeres bejelentkezés után beállítjuk, hogy be van jelentkezve
    setShowLogout(false);  // Bezárjuk a bejelentkezési modalt
  };

  return (
    <div>
      <Navbar cartSize={cart.length} setCategory={setCategory} setSearchQuery={setSearchQuery} handleCategoryChange={handleCategoryChange} setShowCart={setShowCart} setShowLogin={setShowLogin}  isLoggedIn={isLoggedIn}  setShowLogout={setShowLogout}  isLoggedOut={isLoggedOut} setIsLoggedIn={setIsLoggedIn}/>
      <Banner />
      <ProductGrid products={products} category={category} selectedCategory={selectedCategory} searchQuery={searchQuery} addToCart={addToCart} />
      {showCart && <Cart cart={cart} removeFromCart={removeFromCart} setShowCart={setShowCart} />}
      {showLogin && <LoginModal setShowLogin={setShowLogin} setShowRegistration={setShowRegistration} onLoginSuccess={handleLoginSuccess} />} {/* 🔹 Bejelentkezési modal */}
      {showRegistration && <RegistrationModal setShowRegistration={setShowRegistration} />} {/* 🔹 Bejelentkezési modal */}
      {showLogout && <LogoutModal setShowLogout={setShowLogout} onLogoutSuccess={handleLogoutSuccess} />}
    </div>
  );
};

//Navigációs sáv
const Navbar = ({ cartSize,setIsLoggedIn, setSearchQuery, handleCategoryChange, setShowCart, setShowLogin, isLoggedIn,searchQuery }) => {
  const handleSearchChange = (e) => {
    setSearchQuery(e.target.value);
  };
  const handleLogout = async () => {
    try {
      await fetch(`https://localhost:7117/api/Logout/${localStorage.getItem("token").replace(/"/g, "")}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      });

      // Kijelentkezés után eltávolítjuk a felhasználói adatokat és frissítjük az állapotot
      localStorage.removeItem("token");
      setIsLoggedIn(false);
      alert("Sikeres kijelentkezés!");
    } catch (error) {
      console.error("Nem sikerült kijelentkezni!", error);
    }
  };

  return (
    <nav className="navbar-right">
      <div className="logo">Pólók</div>
      <ul className="nav-links">
        <li>
          <a href="#" onClick={(e) => handleCategoryChange(e, "none", "Összes póló")}>Összes</a>
        </li>
        <li>
          <a href="#" onClick={(e) => handleCategoryChange(e, "Női", "Női pólók")}>Női</a>
        </li>
        <li>
          <a href="#" onClick={(e) => handleCategoryChange(e, "Férfi", "Férfi pólók")}>Férfi</a>
        </li>
        <li>
          <a href="#" onClick={(e) => handleCategoryChange(e, "Unisex", "Unisex pólók")}>Unisex</a>
        </li>
      </ul>
      <div className="search-bar">
        <input className="search-input" type="text" placeholder="Keresés..." value={searchQuery} onChange={handleSearchChange} />
      </div>
      {isLoggedIn && (
       <>
       <div className="cart-link" onClick={() => setShowCart(true)} style={{ cursor: "pointer" }}>
         Kosár: {cartSize}
       </div>
       <div className="logout-link" onClick={handleLogout} style={{ cursor: "pointer", color: "red", marginLeft: "15px" }}>
            Kijelentkezés
          </div>
     </>
      )}
      {!isLoggedIn && (
      <div className="login-link" onClick={() => setShowLogin(true)} style={{ cursor: "pointer" }}>
        Bejelentkezés
      </div>
      )}
    </nav>
  );
};

//Felirat
const Banner = () => {
  return (
    <section className="banner">
      <h2>Fedezd fel a legújabb pólóinkat!</h2>
      <p>Válassz a legfrissebb trendek közül és vásárolj online.</p>
    </section>
  );
};



//Termékek
const ProductGrid = ({ products, category, selectedCategory, searchQuery, addToCart }) => {
  const filteredProducts = products.filter(
    (product) =>
      (category === "none" || product.kategoria === category) &&
      product.termekNeve.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <section className="featured-products">
      <h2>{selectedCategory}</h2>
      <div className="product-grid">
        {filteredProducts.map((product) => (
          <div key={product.id} className="product-item">
            <img src={`https://localhost:7117/api/Image/ProductImages/GetImageByName/${product.kep}`} alt={product.termekNeve} />
            <h3>{product.termekNeve}</h3>
            <p>{product.ar.toLocaleString()} Ft</p>
            <div className="product-options">
              <select id={`size-${product.id}`}>
                {JSON.parse(product.meret).map((size) => (
                  <option key={size} value={size}>
                    {size}
                  </option>
                ))}
              </select>
              <input type="number" id={`quantity-${product.id}`} defaultValue="1" min="1" max="10" />
            </div>
            <button
              className="buy-now"
              onClick={() =>
                addToCart(
                  product,
                  document.getElementById(`size-${product.id}`).value,
                  parseInt(document.getElementById(`quantity-${product.id}`).value)
                )
              }
            >
              Kosárba
            </button>
          </div>
        ))}
      </div>
    </section>
  );
};

//Kosár
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

//Kijelentkezés
const LogoutModal = ({ setShowLogout, onLogoutSuccess, setIsLoggedIn }) => {
  const handleLogout = async () => {
    try {
      await fetch(`https://localhost:7117/api/Logout/${localStorage.getItem("token")}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      });
      setShowLogout(false);
      onLogoutSuccess();
    } catch (error) {
      console.error("Nem sikerült kijelentkezni!", error);
    }
  };
//write a function that will be called when the user logs out
  return (
    <div className="logout-modal">
      <div className="logout-modal-content">
        <span className="close-logout" onClick={() => setShowLogout(false)}>×</span>
        <h2>Kijelentkezés</h2>
        <button onClick={handleLogout}>Kijelentkezés</button>
      </div>
    </div>
  );
};


//Bejelentkezés
const LoginModal = ({ setShowLogin, setShowRegistration, onLoginSuccess, token}) => {
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
      const tmpHash = await hashPassword(password, salt); // `hashPassword` egy hashelő függvény (pl. SHA256 vagy egyéb)

      // Most küldjük el a helyes formátumban
      const loginResponse = await fetch("https://localhost:7117/api/Login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ loginName: username, tmpHash }), // A hashelt jelszó és a felhasználónév
      });

      const loginTextData = await loginResponse.text(); // A válasz szöveges formában

      // Ellenőrizzük, hogy a válasz JSON-e
      let data = {};
      try {
        data = JSON.parse(loginTextData); // Próbáljuk JSON-ként értelmezni
      } catch (err) {
        // Ha nem JSON, akkor nem próbáljuk értelmezni JSON-ként
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
      console.log(localStorage.getItem("token"));

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

// Hashelési funkció például SHA256
const hashPassword = async (password, salt) => {
  const encoder = new TextEncoder();
  const data = encoder.encode(password + salt);
  const hashBuffer = await crypto.subtle.digest("SHA-256", data); // Hashing
  const hashArray = Array.from(new Uint8Array(hashBuffer)); // Array from buffer
  return hashArray.map(byte => byte.toString(16).padStart(2, '0')).join(''); // Hex formátum
};

//Regisztráció
const RegistrationModal = ({ setShowRegistration }) => {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState(""); // Hibaüzenet állapota

  const handleregSubmit = async (e) => {
    e.preventDefault();
    setError(""); // Reseteljük az esetleges előző hibát

    if (!username || !password || !email) {
      setError("Felhasználónév,email és jelszó megadása kötelező!");
      return;
    }
    if (password !== confirmPassword) {
      setError("A két jelszó nem egyezik! ❌"); // Hibaüzenet beállítása
      return;
    }

    try {
      // Először lekérjük a sót a `GetSalt` végpontról
      const saltResponse = await fetch(`https://localhost:7117/api/Registration/GetNewSalt`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (!saltResponse.ok) {
        throw new Error("Nem sikerült lekérni a sót!");
      }

      const salt = await saltResponse.text(); // A válasz szöveges formában

      // Ha a só üres, hibát dobunk
      if (!salt || salt === 'null') {
        throw new Error("A só nem érvényes vagy üres.");
      }

      // A jelszó és a só hashelése
      const tmpHash = await hashPassword(password, salt); // `hashPassword` egy hashelő függvény (pl. SHA256 vagy egyéb)

      // Most küldjük el a helyes formátumban
      const registerResponse = await fetch("https://localhost:7117/api/Registration/UserRegistration", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ id: 0, loginName: username, email: email, szamlazasiCimId: 1, salt: salt, hash: tmpHash, active: 0, registarionDate: "2025-02-10T16:13:55.748Z", permissionLevel: 0, szamlazasiCim: null }), // A hashelt jelszó és a felhasználónév
      });

      const registerTextData = await registerResponse.text(); // A válasz szöveges formában

      // Ellenőrizzük, hogy a válasz JSON-e
      const contentType = registerResponse.headers.get("Content-Type");
      let data = {};

      if (contentType && contentType.includes("application/json")) {
        data = await registerResponse.json(); // JSON válasz esetén
      } else {
        data.message = registerTextData; // Ha nem JSON, akkor csak szöveg
      }


      if (!registerResponse.ok) {
        throw new Error(data.message || "Hibás bejelentkezési adatok!");
      }

      // Sikeres bejelentkezés esetén elmentjük az adatokat
      localStorage.setItem("user", JSON.stringify(data.user));
      setError(""); // Ha nincs hiba, töröljük a hibaüzenetet
      alert("Sikeres regisztráció! ✅"); // Ide jöhetne egy API hívás is
      setShowRegistration(false);
      window.location.reload(); // Frissítjük az oldalt a változások megjelenítéséhez
    } catch (err) {
      setError(err.message); // A hibát jelenítjük meg
    }
  };

  return (
    <div className="registration-modal">
      <div className="registration-modal-content">
        <span className="close-registration" onClick={() => setShowRegistration(false)}>×</span>
        <h2>Regisztráció</h2>
        <form onSubmit={handleregSubmit}> {/* Megfelelő függvény */}
          <label>Felhasználónév:</label>
          <input type="text" placeholder="Felhasználónév" required value={username} onChange={(e) => setUsername(e.target.value)} />

          <label>E-mail:</label>
          <input type="email" placeholder="E-mail" required value={email} onChange={(e) => setEmail(e.target.value)} />

          <label>Jelszó:</label>
          <input type="password" placeholder="Jelszó" required value={password} onChange={(e) => setPassword(e.target.value)} />

          <label>Jelszó megerősítés:</label>
          <input type="password" placeholder="Jelszó újra" required value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} />

          {error && <p style={{ color: "red", fontSize: "14px", marginTop: "5px" }}>{error}</p>} {/* Hibaüzenet */}

          <button type="submit">Regisztráció</button>
        </form>
      </div>
    </div>
  );
};

export default App;
