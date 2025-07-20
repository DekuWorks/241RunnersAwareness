import React, { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';

const Shop = () => {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [collections, setCollections] = useState([]);
  const [campaigns, setCampaigns] = useState([]);
  const [partnerships, setPartnerships] = useState([]);
  const [loading, setLoading] = useState(true);
  const [cart, setCart] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [selectedCollection, setSelectedCollection] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('name_asc');
  const [currentPage, setCurrentPage] = useState(1);
  const [showCart, setShowCart] = useState(false);

  useEffect(() => {
    fetchShopData();
  }, []);

  const fetchShopData = async () => {
    try {
      setLoading(true);
      
      // Fetch all data in parallel
      const [productsRes, categoriesRes, collectionsRes, campaignsRes, partnershipsRes] = await Promise.all([
        fetch('/api/shop/products'),
        fetch('/api/shop/categories'),
        fetch('/api/shop/collections'),
        fetch('/api/shop/campaigns'),
        fetch('/api/shop/partnerships')
      ]);

      const [productsData, categoriesData, collectionsData, campaignsData, partnershipsData] = await Promise.all([
        productsRes.json(),
        categoriesRes.json(),
        collectionsRes.json(),
        campaignsRes.json(),
        partnershipsRes.json()
      ]);

      setProducts(productsData.Products || []);
      setCategories(categoriesData || []);
      setCollections(collectionsData || []);
      setCampaigns(campaignsData || []);
      setPartnerships(partnershipsData || []);
    } catch (error) {
      console.error('Failed to fetch shop data:', error);
      toast.error('Failed to load shop data');
    } finally {
      setLoading(false);
    }
  };

  const addToCart = (product) => {
    const existingItem = cart.find(item => item.id === product.Id);
    
    if (existingItem) {
      setCart(cart.map(item => 
        item.id === product.Id 
          ? { ...item, quantity: item.quantity + 1 }
          : item
      ));
    } else {
      setCart([...cart, {
        id: product.Id,
        name: product.Name,
        price: product.CurrentPrice,
        image: product.ImageUrls?.split(',')[0] || '/images/placeholder.jpg',
        quantity: 1,
        donationAmount: product.DonationAmount || 0
      }]);
    }
    
    toast.success(`${product.Name} added to cart`);
  };

  const removeFromCart = (productId) => {
    setCart(cart.filter(item => item.id !== productId));
    toast.success('Item removed from cart');
  };

  const updateCartQuantity = (productId, quantity) => {
    if (quantity <= 0) {
      removeFromCart(productId);
      return;
    }
    
    setCart(cart.map(item => 
      item.id === productId 
        ? { ...item, quantity }
        : item
    ));
  };

  const getCartTotal = () => {
    return cart.reduce((total, item) => total + (item.price * item.quantity), 0);
  };

  const getCartDonationTotal = () => {
    return cart.reduce((total, item) => total + (item.donationAmount * item.quantity), 0);
  };

  const filteredProducts = products.filter(product => {
    const matchesCategory = !selectedCategory || product.Category === selectedCategory;
    const matchesCollection = !selectedCollection || product.Collection === selectedCollection;
    const matchesSearch = !searchTerm || 
      product.Name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.Description.toLowerCase().includes(searchTerm.toLowerCase());
    
    return matchesCategory && matchesCollection && matchesSearch;
  });

  const sortedProducts = [...filteredProducts].sort((a, b) => {
    switch (sortBy) {
      case 'price_asc':
        return a.CurrentPrice - b.CurrentPrice;
      case 'price_desc':
        return b.CurrentPrice - a.CurrentPrice;
      case 'name_asc':
        return a.Name.localeCompare(b.Name);
      case 'name_desc':
        return b.Name.localeCompare(a.Name);
      case 'newest':
        return new Date(b.CreatedAt) - new Date(a.CreatedAt);
      default:
        return a.Name.localeCompare(b.Name);
    }
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="shop-container">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-600 to-red-600 text-white py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-4xl font-bold mb-4">241RA x Varlo Shop</h1>
            <p className="text-xl mb-6">Premium athletic wear supporting missing persons awareness</p>
            <div className="flex justify-center space-x-4">
              <button 
                onClick={() => setShowCart(true)}
                className="bg-white text-blue-600 px-6 py-2 rounded-lg font-semibold hover:bg-gray-100 transition-colors"
              >
                🛒 Cart ({cart.length})
              </button>
              <button className="bg-red-600 text-white px-6 py-2 rounded-lg font-semibold hover:bg-red-700 transition-colors">
                💝 Donate Now
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Campaigns Section */}
      <div className="bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Active Campaigns</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {campaigns.map((campaign, index) => (
              <div key={index} className="bg-white rounded-lg shadow-md p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-semibold text-gray-900">{campaign.Name}</h3>
                  <span className="text-sm text-gray-500">{campaign.CampaignType}</span>
                </div>
                <p className="text-gray-600 mb-4">{campaign.Description}</p>
                <div className="mb-4">
                  <div className="flex justify-between text-sm mb-2">
                    <span>Progress</span>
                    <span>${campaign.CurrentAmount.toLocaleString()} / ${campaign.GoalAmount.toLocaleString()}</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${(campaign.CurrentAmount / campaign.GoalAmount) * 100}%` }}
                    ></div>
                  </div>
                </div>
                <button className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 transition-colors">
                  Support Campaign
                </button>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Partnerships Section */}
      <div className="py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Our Partners</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {partnerships.map((partnership, index) => (
              <div key={index} className="bg-white rounded-lg shadow-md p-6 text-center">
                <div className="w-16 h-16 bg-gray-200 rounded-full mx-auto mb-4 flex items-center justify-center">
                  <span className="text-2xl">🏢</span>
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">{partnership.PartnerName}</h3>
                <p className="text-gray-600 mb-4">{partnership.Description}</p>
                <span className="inline-block bg-blue-100 text-blue-800 text-sm px-3 py-1 rounded-full">
                  {partnership.PartnershipType}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white border-b py-6">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <input
                type="text"
                placeholder="Search products..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Categories</option>
              {categories.map((category, index) => (
                <option key={index} value={category.Name}>{category.Name}</option>
              ))}
            </select>
            <select
              value={selectedCollection}
              onChange={(e) => setSelectedCollection(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All Collections</option>
              {collections.map((collection, index) => (
                <option key={index} value={collection.Name}>{collection.Name}</option>
              ))}
            </select>
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="name_asc">Name A-Z</option>
              <option value="name_desc">Name Z-A</option>
              <option value="price_asc">Price Low-High</option>
              <option value="price_desc">Price High-Low</option>
              <option value="newest">Newest First</option>
            </select>
          </div>
        </div>
      </div>

      {/* Products Grid */}
      <div className="py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {sortedProducts.map((product) => (
              <div key={product.Id} className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow">
                <div className="aspect-w-1 aspect-h-1 bg-gray-200">
                  <img
                    src={product.ImageUrls?.split(',')[0] || '/images/placeholder.jpg'}
                    alt={product.Name}
                    className="w-full h-48 object-cover"
                  />
                </div>
                <div className="p-4">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm text-gray-500">{product.Brand}</span>
                    {product.IsOnSale && (
                      <span className="bg-red-500 text-white text-xs px-2 py-1 rounded">SALE</span>
                    )}
                  </div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">{product.Name}</h3>
                  <p className="text-gray-600 text-sm mb-3">{product.Description}</p>
                  
                  {product.DonationAmount > 0 && (
                    <div className="bg-green-50 border border-green-200 rounded-md p-2 mb-3">
                      <p className="text-green-800 text-sm">
                        💝 ${product.DonationAmount} donation per item
                      </p>
                    </div>
                  )}
                  
                  <div className="flex items-center justify-between mb-3">
                    <div>
                      {product.SalePrice ? (
                        <div>
                          <span className="text-lg font-bold text-red-600">${product.SalePrice}</span>
                          <span className="text-sm text-gray-500 line-through ml-2">${product.Price}</span>
                        </div>
                      ) : (
                        <span className="text-lg font-bold text-gray-900">${product.Price}</span>
                      )}
                    </div>
                    <span className="text-sm text-gray-500">{product.Color}</span>
                  </div>
                  
                  <button
                    onClick={() => addToCart(product)}
                    disabled={!product.IsInStock}
                    className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                  >
                    {product.IsInStock ? 'Add to Cart' : 'Out of Stock'}
                  </button>
                </div>
              </div>
            ))}
          </div>
          
          {sortedProducts.length === 0 && (
            <div className="text-center py-12">
              <p className="text-gray-500 text-lg">No products found matching your criteria.</p>
            </div>
          )}
        </div>
      </div>

      {/* Cart Sidebar */}
      {showCart && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50">
          <div className="fixed right-0 top-0 h-full w-96 bg-white shadow-lg">
            <div className="p-6">
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-xl font-bold">Shopping Cart</h2>
                <button
                  onClick={() => setShowCart(false)}
                  className="text-gray-500 hover:text-gray-700"
                >
                  ✕
                </button>
              </div>
              
              {cart.length === 0 ? (
                <p className="text-gray-500 text-center py-8">Your cart is empty</p>
              ) : (
                <div>
                  <div className="space-y-4 mb-6">
                    {cart.map((item) => (
                      <div key={item.id} className="flex items-center space-x-4">
                        <img
                          src={item.image}
                          alt={item.name}
                          className="w-16 h-16 object-cover rounded"
                        />
                        <div className="flex-1">
                          <h3 className="font-semibold">{item.name}</h3>
                          <p className="text-gray-500">${item.price}</p>
                        </div>
                        <div className="flex items-center space-x-2">
                          <button
                            onClick={() => updateCartQuantity(item.id, item.quantity - 1)}
                            className="w-6 h-6 bg-gray-200 rounded flex items-center justify-center"
                          >
                            -
                          </button>
                          <span>{item.quantity}</span>
                          <button
                            onClick={() => updateCartQuantity(item.id, item.quantity + 1)}
                            className="w-6 h-6 bg-gray-200 rounded flex items-center justify-center"
                          >
                            +
                          </button>
                        </div>
                        <button
                          onClick={() => removeFromCart(item.id)}
                          className="text-red-500 hover:text-red-700"
                        >
                          ✕
                        </button>
                      </div>
                    ))}
                  </div>
                  
                  <div className="border-t pt-4">
                    <div className="flex justify-between mb-2">
                      <span>Subtotal:</span>
                      <span>${getCartTotal().toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between mb-2 text-green-600">
                      <span>Donation:</span>
                      <span>${getCartDonationTotal().toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between font-bold text-lg">
                      <span>Total:</span>
                      <span>${(getCartTotal() + getCartDonationTotal()).toFixed(2)}</span>
                    </div>
                  </div>
                  
                  <button className="w-full bg-blue-600 text-white py-3 rounded-md mt-4 hover:bg-blue-700 transition-colors">
                    Proceed to Checkout
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Styles */}
      <style jsx>{`
        .shop-container {
          min-height: 100vh;
          background: #f8fafc;
        }
      `}</style>
    </div>
  );
};

export default Shop; 