import { useState } from "react";

// Regisztráció
const RegistrationModal = ({ setShowRegistration }) => {
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [firstName, setFirstName] = useState(""); // First Name state
    const [lastName, setLastName] = useState(""); // Last Name state


    const [error, setError] = useState(""); // Hibaüzenet állapota

// Hashelési funkció például SHA256
const hashPassword = async (password, salt) => {
    const encoder = new TextEncoder();
    const data = encoder.encode(password + salt);
    const hashBuffer = await crypto.subtle.digest("SHA-256", data); // Hashing
    const hashArray = Array.from(new Uint8Array(hashBuffer)); // Array from buffer
    return hashArray.map(byte => byte.toString(16).padStart(2, '0')).join(''); // Hex formátum
  };



    const handleregSubmit = async (e) => {
        e.preventDefault();
        setError(""); // Reseteljük az esetleges előző hibát

        if (!username || !password || !email || !firstName || !lastName ) {
            setError("Felhasználónév, e-mail, jelszó, név megadása kötelező!");
            return;
        }
        if (password !== confirmPassword) {
            setError("A két jelszó nem egyezik! ❌");
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

            const salt = await saltResponse.text();

            if (!salt || salt === 'null') {
                throw new Error("A só nem érvényes vagy üres.");
            }

            const tmpHash = await hashPassword(password, salt); // Hashelés a jelszóval és sóval

            const currentDate = new Date().toISOString(); // Dinamikusan generált regisztráció dátum

            // Regisztráció végpont hívás
            const registerResponse = await fetch("https://localhost:7117/api/Registration/UserRegistration", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                   
                    loginName: username,
                    email: email,
                    firstName: firstName, // Added First Name
                    lastName: lastName,   // Added Last Name
                   
                    salt: salt,
                    tempHash: tmpHash,
                    
                    
                }),
            });

            const registerTextData = await registerResponse.text();

            const contentType = registerResponse.headers.get("Content-Type");
            let data = {};

            if (contentType && contentType.includes("application/json")) {
                data = await registerResponse.json();
            } else {
                data.message = registerTextData;
            }

            if (!registerResponse.ok) {
                throw new Error(data.message || "Hibás bejelentkezési adatok!");
            }

            localStorage.setItem("user", JSON.stringify(data.user)); // Felhasználói adatokat elmentjük
            setError(""); // Töröljük a hibaüzenetet
            alert("Sikeres regisztráció! ✅");
            setShowRegistration(false);
            window.location.reload();
        } catch (err) {
            setError(err.message); // Hibát jelenítünk meg
        }
    };

    return (
        <div className="registration-modal">
            <div className="registration-modal-content">
                <span className="close-reg" onClick={() => setShowRegistration(false)}>×</span>
                <h2>Regisztráció</h2>
                <form onSubmit={handleregSubmit} className="registration-form">
                    <div className="form-group">
                        <label>Felhasználónév:</label>
                        <input
                            type="text"
                            placeholder="Felhasználónév"
                            required
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                        />
                    </div>

                    <div className="form-group">
                        <label>E-mail:</label>
                        <input
                            type="email"
                            placeholder="E-mail"
                            required
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>

                    <div className="form-group">
                        <label>Vezetéknév:</label>
                        <input
                            type="text"
                            placeholder="Vezetéknév"
                            required
                            value={lastName}
                            onChange={(e) => setLastName(e.target.value)}
                        />
                    </div>

                    <div className="form-group">
                        <label>Keresztnév:</label>
                        <input
                            type="text"
                            placeholder="Keresztnév"
                            required
                            value={firstName}
                            onChange={(e) => setFirstName(e.target.value)}
                        />
                    </div>

                

               

                    <div className="form-group">
                        <label>Jelszó:</label>
                        <input
                            type="password"
                            placeholder="Jelszó"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>

                    <div className="form-group">
                        <label>Jelszó megerősítés:</label>
                        <input
                            type="password"
                            placeholder="Jelszó újra"
                            required
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                    </div>

                    {error && (
                        <p style={{ color: "red", fontSize: "14px", marginTop: "5px" }}>
                            {error}
                        </p>
                    )}

                    <button type="submit">Regisztráció</button>
                </form>
            </div>
        </div>
    );
};

export default RegistrationModal;
