from selenium import webdriver
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.common.by import By
import time
options = webdriver.ChromeOptions()
driver = webdriver.Chrome(options=options)
driver.set_window_size(1920, 1080)

try:
    print("1️⃣ Megnyitjuk az oldalt...")
    driver.get("http://localhost:3000")  # 🚀 Állítsd be a saját React app URL-jére
    print("Oldal betöltődött.")

    # 2️⃣ Megvárjuk és kattintunk a "Bejelentkezés" gombra
    print("Várakozás a 'Bejelentkezés' gombra...")
    login_link = driver.find_element(By.CLASS_NAME, "login-link")
    login_link.click()
    print("Kattintottunk a 'Bejelentkezés' gombra.")

    # 3️⃣ Megvárjuk, hogy a bejelentkezési modal megjelenjen
    print("Várakozás a felhasználónév és jelszó mezőre...")
    username_input = driver.find_element(By.CLASS_NAME, "username").send_keys("user1")
    password_input = driver.find_element(By.CLASS_NAME, "password").send_keys("Titkos1")
    print("Felhasználónév és jelszó mező megtalálva, beírtuk a felhasználónevet és a jelszót.")

    # 5️⃣ Rákattintunk a belépés gombra
    login_button = driver.find_element(By.XPATH, "//button[text()='Belépés']")
    login_button.click()
    print("Rákattintottunk a belépés gombra.")
    time.sleep(3)
    alert = driver.switch_to.alert  # Átváltunk az alertre

    # Az alert bezárása
    alert.accept()  # Accept (OK gomb) az alert bezárásához

    time.sleep(3)
    driver.save_screenshot('login.png')
    time.sleep(3)
    
    # Ha sikerült, kiíratjuk
    print("✅ A teszt sikeresen lefutott!")
    
except Exception as e:
    # Hibakezelés: Részletes hibaüzenet kiírása
    print(f"❌ Hiba történt: {e}")

finally:
    # 🔥 Bezárjuk a böngészőt
    driver.quit()
