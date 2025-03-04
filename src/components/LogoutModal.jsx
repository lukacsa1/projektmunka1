const LogoutModal = ({ setShowLogout, onLogoutSuccess, setIsLoggedIn }) => {
    const handleLogout = async () => {
      try {
        await fetch(`https://localhost:7117/api/Logout/${localStorage.getItem("token")}`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
        });
        localStorage.removeItem("token"); // Töröljük a tokent a helyi tárolóból
        setIsLoggedIn(false); // Frissítjük a bejelentkezett státuszt
        setShowLogout(false); // Bezárjuk a logout modált
        onLogoutSuccess(); // Ha szükséges, visszahívjuk a sikeres kijelentkezést
      } catch (error) {
        console.error("Nem sikerült kijelentkezni!", error);
      }
    };

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

export default LogoutModal;
