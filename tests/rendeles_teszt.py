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
    print("Felhasználónév és jelszó mező megtalálva.")
    print("Beírtuk a felhasználónevet és a jelszót.")

    # 5️⃣ Rákattintunk a belépés gombra
    login_button = driver.find_element(By.XPATH, "//button[text()='Belépés']")
    login_button.click()
    print("Rákattintottunk a belépés gombra.")
    time.sleep(3)
    alert = driver.switch_to.alert  # Átváltunk az alertre

    # Az alert bezárása
    alert.accept()  # Accept (OK gomb) az alert bezárásához

    print("Várakozás a 'Kosárba' gombra...")
    kosarba = driver.find_element(By.CLASS_NAME, "buy-now")
    kosarba.click()
    print("Kattintottunk a 'Kosárba' gombra.")

    print("Várakozás a 'Kosár' gombra...")
    cart =  driver.find_element(By.CLASS_NAME, "cart-link")
    cart.click()
    print("Kattintottunk a 'Kosár' gombra.")

    print("Várakozás a 'Pénztárhoz' gombra...")
    penztar =  driver.find_element(By.CLASS_NAME, "checkout-btn")
    penztar.click()
    print("Kattintottunk a 'Pénztárhoz' gombra.")

    name =  driver.find_element(By.NAME, "name").send_keys("kis")
    email = driver.find_element(By.NAME, "email").send_keys("gabi")
    phone = driver.find_element(By.NAME, "phoneNumber").send_keys("valaki@gmail.com")
    postal = driver.find_element(By.NAME, "postalCode").send_keys(1234)
    country = driver.find_element(By.NAME, "country").send_keys("Magyarország")
    city = driver.find_element(By.NAME, "city").send_keys("Alsózsolca")
    street = driver.find_element(By.NAME, "street").send_keys("Béla utca")
    house = driver.find_element(By.NAME, "houseNumber").send_keys("13")
    print("Beírtuk az adatokat.")


    time.sleep(3)
    driver.save_screenshot('rendeles.png')
    time.sleep(3)
    # Ha sikerült, kiíratjuk
    print("✅ A teszt sikeresen lefutott!")

except Exception as e:
    # Hibakezelés: Részletes hibaüzenet kiírása
    print(f"❌ Hiba történt: {e}")

finally:
    # 🔥 Bezárjuk a böngészőt
    driver.quit()
