import React from "react";

const ProductGrid = ({ products, category, selectedCategory, searchQuery, addToCart }) => {
  // Szűrés a kategória és keresési szempontok alapján
  const filteredProducts = products.filter(
    (product) =>
      (category === "none" || product.kategoria === category) &&
      product.termekNeve.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // Termékek listázása
  return (
    <section className="featured-products">
      <h2>{selectedCategory}</h2>
      <div className="product-grid">
        {filteredProducts.map((product) => {
          // Kiválasztott méret és mennyiség
          const handleAddToCart = () => {
            const size = document.getElementById(`size-${product.id}`).value;
            const quantity = parseInt(document.getElementById(`quantity-${product.id}`).value);
            addToCart(product, size, quantity);
          };

          return (
            <div key={product.id} className="product-item">
              <img
                src={`https://localhost:7117/api/Image/ProductImages/GetImageByName/${product.kep}`}
                alt={product.termekNeve}
              />
              <h3>{product.termekNeve}</h3>
              <p>{product.ar.toLocaleString()} Ft</p>
              <div className="product-options">
                <select id={`size-${product.id}`} defaultValue={product.meret[0]}>
                  {JSON.parse(product.meret).map((size) => (
                    <option key={size} value={size}>
                      {size}
                    </option>
                  ))}
                </select>
                <input
                  type="number"
                  id={`quantity-${product.id}`}
                  defaultValue="1"
                  min="1"
                  max="10"
                />
              </div>
              <button className="buy-now" onClick={handleAddToCart}>
                Kosárba
              </button>
            </div>
          );
        })}
      </div>
    </section>
  );
};

export default ProductGrid;
