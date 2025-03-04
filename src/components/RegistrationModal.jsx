import { useState } from "react";

// Regisztráció
const RegistrationModal = ({ setShowRegistration }) => {
    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState(""); // Hibaüzenet állapota

    // Jelszó hashelés
    const hashPassword = async (password, salt) => {
        // Hashelés logika itt (pl. SHA256 vagy egyéb algoritmus)
        // Ha nincs hashelés, próbálhatunk más megoldást is
        return password + salt; // Például sóval való egyszerű összefűzés
    }

    const handleregSubmit = async (e) => {
        e.preventDefault();
        setError(""); // Reseteljük az esetleges előző hibát

        if (!username || !password || !email) {
            setError("Felhasználónév, e-mail és jelszó megadása kötelező!");
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

            const registerResponse = await fetch("https://localhost:7117/api/Registration/UserRegistration", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    id: 0,
                    loginName: username,
                    email: email,
                    szamlazasiCimId: 1,
                    salt: salt,
                    hash: tmpHash,
                    active: 0,
                    registarionDate: currentDate,
                    permissionLevel: 0,
                    szamlazasiCim: null,
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

            localStorage.setItem("user", JSON.stringify(data.user));
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
                <span className="close-registration" onClick={() => setShowRegistration(false)}>×</span>
                <h2>Regisztráció</h2>
                <form onSubmit={handleregSubmit}>
                    <label>Felhasználónév:</label>
                    <input
                        type="text"
                        placeholder="Felhasználónév"
                        required
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />

                    <label>E-mail:</label>
                    <input
                        type="email"
                        placeholder="E-mail"
                        required
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />

                    <label>Jelszó:</label>
                    <input
                        type="password"
                        placeholder="Jelszó"
                        required
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />

                    <label>Jelszó megerősítés:</label>
                    <input
                        type="password"
                        placeholder="Jelszó újra"
                        required
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                    />

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
