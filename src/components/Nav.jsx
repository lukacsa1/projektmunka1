const Navbar = ({
    cartSize,
    setCategory,
    setSearchQuery,
    searchQuery,
    setShowCart,
    setShowLogin,
    isLoggedIn,
    setShowLogout,
    setIsLoggedIn,
    setShowProfile // Itt is hozzáadjuk a setShowProfile-t
  }) => {
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
            <a href="#" onClick={(e) => setCategory("none")}>Összes</a>
          </li>
          <li>
            <a href="#" onClick={(e) => setCategory("Női")}>Női</a>
          </li>
          <li>
            <a href="#" onClick={(e) => setCategory("Férfi")}>Férfi</a>
          </li>
          <li>
            <a href="#" onClick={(e) => setCategory("Unisex")}>Unisex</a>
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
            <div className="profile-link" onClick={() => setShowProfile(true)} style={{ cursor: "pointer", marginLeft: "15px" }}>
              Profil
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
  
  export default Navbar;  // Itt adunk alapértelmezett exportot
  