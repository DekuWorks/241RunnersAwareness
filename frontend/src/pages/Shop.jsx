import React, { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';

const Shop = () => {
  const [products, setProducts] = useState([]);
  const [filteredProducts, setFilteredProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [filters, setFilters] = useState({
    category: '',
    collection: '',
    sortBy: ''
  });

  // Mock product data - same as static site
  const mockProducts = [
    {
      id: 1,
      name: "241RA x Varlo Heritage Singlet",
      description: "Premium triathlon singlet supporting missing persons awareness",
      price: 89.99,
      salePrice: 69.99,
      category: "Triathlon",
      collection: "Heritage",
      brand: "241RA x Varlo",
      color: "Navy Blue",
      donationAmount: 10.50,
      image: "🏃‍♂️"
    },
    {
      id: 2,
      name: "Mercury Tri Shorts",
      description: "High-performance triathlon shorts with 241RA awareness",
      price: 129.99,
      category: "Triathlon",
      collection: "Mercury",
      brand: "Varlo",
      color: "Black",
      donationAmount: 13.00,
      image: "🩳"
    },
    {
      id: 3,
      name: "Wildbloom Awareness Tee",
      description: "Everyday comfort with mission-driven design",
      price: 45.99,
      salePrice: 35.99,
      category: "Everyday",
      collection: "Wildbloom",
      brand: "241RA x Varlo",
      color: "White",
      donationAmount: 7.20,
      image: "👕"
    },
    {
      id: 4,
      name: "Law Enforcement Support Jersey",
      description: "Dedicated to those who protect and serve",
      price: 79.99,
      category: "Everyday",
      collection: "Law Enforcement Support",
      brand: "241RA",
      color: "Dark Blue",
      donationAmount: 20.00,
      image: "👮‍♂️"
    },
    {
      id: 5,
      name: "DNA Collection Kit",
      description: "Professional DNA collection kit for missing persons cases",
      price: 29.99,
      category: "Accessories",
      collection: "241RA Awareness",
      brand: "241RA",
      color: "White",
      donationAmount: 5.00,
      image: "🧬"
    },
    {
      id: 6,
      name: "Awareness Bracelet",
      description: "Silicone awareness bracelet with 241RA mission",
      price: 12.99,
      category: "Accessories",
      collection: "241RA Awareness",
      brand: "241RA",
      color: "Red",
      donationAmount: 3.00,
      image: "📿"
    }
  ];

  useEffect(() => {
    setProducts(mockProducts);
    setFilteredProducts(mockProducts);
  }, []);

  useEffect(() => {
    filterProducts();
  }, [filters, products]);

  const filterProducts = () => {
    let filtered = [...products];

    if (filters.category) {
      filtered = filtered.filter(product => product.category === filters.category);
    }

    if (filters.collection) {
      filtered = filtered.filter(product => product.collection === filters.collection);
    }

    // Sort products
    if (filters.sortBy === 'name') {
      filtered.sort((a, b) => a.name.localeCompare(b.name));
    } else if (filters.sortBy === 'price-low') {
      filtered.sort((a, b) => (a.salePrice || a.price) - (b.salePrice || b.price));
    } else if (filters.sortBy === 'price-high') {
      filtered.sort((a, b) => (b.salePrice || b.price) - (a.salePrice || a.price));
    }

    setFilteredProducts(filtered);
  };

  const addToCart = (productId) => {
    const product = products.find(p => p.id === productId);
    const existingItem = cart.find(item => item.id === productId);

    if (existingItem) {
      setCart(cart.map(item => 
        item.id === productId 
          ? { ...item, quantity: item.quantity + 1 }
          : item
      ));
    } else {
      setCart([...cart, { ...product, quantity: 1 }]);
    }

    toast.success(`${product.name} added to cart`);
  };

  const updateQuantity = (productId, newQuantity) => {
    if (newQuantity <= 0) {
      removeFromCart(productId);
      return;
    }

    setCart(cart.map(item => 
      item.id === productId 
        ? { ...item, quantity: newQuantity }
        : item
    ));
  };

  const removeFromCart = (productId) => {
    setCart(cart.filter(item => item.id !== productId));
    toast.success('Item removed from cart');
  };

  const getCartTotal = () => {
    const subtotal = cart.reduce((total, item) => total + (item.salePrice || item.price) * item.quantity, 0);
    const donation = cart.reduce((total, item) => total + item.donationAmount * item.quantity, 0);
    return { subtotal, donation, total: subtotal + donation };
  };

  const checkout = () => {
    if (cart.length === 0) {
      toast.error('Your cart is empty');
      return;
    }

    const { total } = getCartTotal();
    toast.success(`Proceeding to checkout - Total: $${total.toFixed(2)}`);
    // In a real implementation, this would redirect to a payment processor
  };

  const { subtotal, donation, total } = getCartTotal();

  return (
    <div className="shop-container">
      {/* Hero Section */}
      <div className="hero-section">
        <h1>241RA x Varlo Collaboration</h1>
        <p>Premium athletic wear supporting missing persons awareness. Every purchase helps fund DNA collection and identification technology.</p>
        <div className="hero-buttons">
          <button onClick={() => document.getElementById('products').scrollIntoView({ behavior: 'smooth' })}>
            🛍️ Shop Now
          </button>
          <button onClick={() => document.getElementById('dna').scrollIntoView({ behavior: 'smooth' })}>
            🧬 DNA Technology
          </button>
        </div>
      </div>



      {/* DNA Technology Section */}
      <div className="dna-section" id="dna">
        <h2>🧬 DNA Tracking & Identification</h2>
        <p>Advanced DNA technology for missing persons identification. Every purchase supports our DNA collection initiatives.</p>
        
        <div className="dna-grid">
          <div className="dna-card">
            <h3>DNA Collection</h3>
            <p>Secure DNA sample collection and storage for missing persons cases</p>
            <div className="dna-stats">
              <div className="stat-item">
                <span className="stat-number">1,250</span>
                <span className="stat-label">Samples Collected</span>
              </div>
              <div className="stat-item">
                <span className="stat-number">45</span>
                <span className="stat-label">This Month</span>
              </div>
            </div>
          </div>

          <div className="dna-card">
            <h3>Database Integration</h3>
            <p>Integration with NAMUS and CODIS databases for national identification</p>
            <div className="dna-stats">
              <div className="stat-item">
                <span className="stat-number">23</span>
                <span className="stat-label">NAMUS Matches</span>
              </div>
              <div className="stat-item">
                <span className="stat-number">8</span>
                <span className="stat-label">CODIS Matches</span>
              </div>
            </div>
          </div>

          <div className="dna-card">
            <h3>Lab Partnerships</h3>
            <p>Partnership with leading forensic DNA laboratories</p>
            <div className="dna-stats">
              <div className="stat-item">
                <span className="stat-number">6</span>
                <span className="stat-label">Partner Labs</span>
              </div>
              <div className="stat-item">
                <span className="stat-number">98.5%</span>
                <span className="stat-label">Success Rate</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Partners Section */}
      <div className="partners-section">
        <h2>Our Partners</h2>
        <div className="partners-grid">
          <div className="partner-card">
            <div className="partner-logo">🏃‍♂️</div>
            <h3>Varlo</h3>
            <p>Premium athletic apparel collaboration</p>
            <span className="partner-badge merchandise">Merchandise</span>
          </div>

          <div className="partner-card">
            <div className="partner-logo">🔍</div>
            <h3>NAMUS</h3>
            <p>National Missing and Unidentified Persons System</p>
            <span className="partner-badge database">DNA Database</span>
          </div>

          <div className="partner-card">
            <div className="partner-logo">👮‍♂️</div>
            <h3>Houston PD</h3>
            <p>Local law enforcement collaboration</p>
            <span className="partner-badge law-enforcement">Law Enforcement</span>
          </div>
        </div>
      </div>

      {/* Products Section */}
      <div className="products-section" id="products">
        <h2>Shop Products</h2>
        
        <div className="filters">
          <select 
            value={filters.category} 
            onChange={(e) => setFilters({...filters, category: e.target.value})}
          >
            <option value="">All Categories</option>
            <option value="Running">Running</option>
            <option value="Triathlon">Triathlon</option>
            <option value="Cycling">Cycling</option>
            <option value="Everyday">Everyday</option>
            <option value="Accessories">Accessories</option>
            <option value="241RA Awareness">241RA Awareness</option>
          </select>
          
          <select 
            value={filters.collection} 
            onChange={(e) => setFilters({...filters, collection: e.target.value})}
          >
            <option value="">All Collections</option>
            <option value="Heritage">Heritage</option>
            <option value="Mercury">Mercury</option>
            <option value="Wildbloom">Wildbloom</option>
            <option value="Law Enforcement Support">Law Enforcement Support</option>
          </select>
          
          <select 
            value={filters.sortBy} 
            onChange={(e) => setFilters({...filters, sortBy: e.target.value})}
          >
            <option value="">Sort By</option>
            <option value="name">Name A-Z</option>
            <option value="price-low">Price Low-High</option>
            <option value="price-high">Price High-Low</option>
            <option value="newest">Newest First</option>
          </select>
        </div>

        <div className="products-grid">
          {filteredProducts.map(product => (
            <div key={product.id} className="product-card">
              <div className="product-image">{product.image}</div>
              <div className="product-info">
                <div className="product-title">{product.name}</div>
                <div className="product-description">{product.description}</div>
                <div className="donation-badge">💝 ${product.donationAmount} donation per item</div>
                <div className="product-price">
                  <div>
                    {product.salePrice ? (
                      <>
                        <span className="price sale-price">${product.salePrice}</span>
                        <span className="original-price">${product.price}</span>
                      </>
                    ) : (
                      <span className="price">${product.price}</span>
                    )}
                  </div>
                  <span className="product-color">{product.color}</span>
                </div>
                <button 
                  className="add-to-cart-btn"
                  onClick={() => addToCart(product.id)}
                >
                  Add to Cart
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Shopping Cart */}
      <div className="cart-section">
        <div className="cart-header">
          <h3>Shopping Cart ({cart.reduce((total, item) => total + item.quantity, 0)})</h3>
        </div>
        <div className="cart-items">
          {cart.length === 0 ? (
            <p className="empty-cart">Your cart is empty</p>
          ) : (
            cart.map(item => (
              <div key={item.id} className="cart-item">
                <div className="cart-item-image">{item.image}</div>
                <div className="cart-item-info">
                  <div className="cart-item-title">{item.name}</div>
                  <div className="cart-item-price">${(item.salePrice || item.price).toFixed(2)}</div>
                </div>
                <div className="cart-quantity">
                  <button onClick={() => updateQuantity(item.id, item.quantity - 1)}>-</button>
                  <span>{item.quantity}</span>
                  <button onClick={() => updateQuantity(item.id, item.quantity + 1)}>+</button>
                </div>
                <button 
                  className="remove-btn"
                  onClick={() => removeFromCart(item.id)}
                >
                  ✕
                </button>
              </div>
            ))
          )}
        </div>
        {cart.length > 0 && (
          <div className="cart-total">
            <div className="total-row">
              <span>Subtotal:</span>
              <span>${subtotal.toFixed(2)}</span>
            </div>
            <div className="total-row">
              <span>Donation:</span>
              <span>${donation.toFixed(2)}</span>
            </div>
            <div className="total-row total">
              <span>Total:</span>
              <span>${total.toFixed(2)}</span>
            </div>
            <button className="checkout-btn" onClick={checkout}>
              Proceed to Checkout
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default Shop; 