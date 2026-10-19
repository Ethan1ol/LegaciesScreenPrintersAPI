-- 1. USERS TABLE
CREATE TABLE users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(120) NOT NULL UNIQUE,
    phone_number NVARCHAR(20) NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    role NVARCHAR(20) DEFAULT 'customer' CHECK (role IN ('customer', 'admin')),
    created_at DATETIME2 DEFAULT GETDATE()
);

-- 2. MATERIALS TABLE
CREATE TABLE materials (
    material_id INT IDENTITY(1,1) PRIMARY KEY,
    material_name NVARCHAR(100) NOT NULL,
    color NVARCHAR(50) NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    stock_quantity INT NOT NULL,
    unit_measure NVARCHAR(20) DEFAULT 'meters',
    low_stock_threshold INT DEFAULT 15,
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- 3. ORDERS TABLE
CREATE TABLE orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    order_ref NVARCHAR(20) NOT NULL UNIQUE,
    user_id INT NOT NULL,
    order_type NVARCHAR(20) DEFAULT 'mobile' CHECK (order_type IN ('mobile', 'walk_in')),
    total_amount DECIMAL(10,2) NOT NULL,
    status NVARCHAR(50) DEFAULT 'Order Received & Verified' CHECK (
        status IN (
            'Order Received & Verified',
            'Vinyl Cutting & Screen Prep',
            'Screen Printing',
            'Ready for Collection',
            'Completed'
        )
    ),
    created_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 4. ORDER ITEMS TABLE
CREATE TABLE order_items (
    item_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    design_name NVARCHAR(100) NOT NULL,
    artwork_file_url NVARCHAR(255),
    material_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Materials FOREIGN KEY (material_id) REFERENCES materials(material_id)
);

-- INSERT SEED DATA (10 RECORDS PER TABLE)

-- Users
INSERT INTO users (full_name, email, phone_number, password_hash, role) VALUES
('Legacy Admin', 'admin@legaciesprinters.co.za', '0821112233', '$2y$10$e8wV4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'admin'),
('Daniel Smith', 'daniel.smith@gmail.com', '0825551234', '$2y$10$w8uM4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Sipho Ndlovu', 'sipho.ndlovu@outlook.com', '0714449876', '$2y$10$x9vN4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Amina Cassiem', 'amina.c@webmail.co.za', '0833334567', '$2y$10$y1wP4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Liam Jacobs', 'liam.jacobs@gmail.com', '0842223456', '$2y$10$z2xQ4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Chantelle Meyer', 'chantelle.m@icloud.com', '0761118765', '$2y$10$a3yR4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Thabo Mokoena', 'thabo.mokoena@gmail.com', '0829995432', '$2y$10$b4zS4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Nicole Van Zyl', 'nicole.vz@yahoo.com', '0798881234', '$2y$10$c5aT4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Devon Hendricks', 'devon.h@gmail.com', '0817776543', '$2y$10$d6bU4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer'),
('Fatima Patel', 'fatima.patel@mweb.co.za', '0736665432', '$2y$10$e7cV4113jQvK1B7/8P4q7.22iQf8u2F9d7Z7F.6wYw9sXyG8bA7cW', 'customer');

-- Materials
INSERT INTO materials (material_name, color, unit_price, stock_quantity, low_stock_threshold) VALUES
('Gloss Vinyl Standard', 'Gloss White', 45.00, 120, 20),
('Gloss Vinyl Standard', 'Jet Black', 45.00, 85, 20),
('Matte Vinyl Premium', 'Matte Gold', 65.00, 8, 15),
('Matte Vinyl Premium', 'Matte Black', 60.00, 110, 20),
('Heavy Duty Banner Vinyl (10oz)', 'White / Gold', 75.00, 42, 10),
('Reflective Safety Vinyl', 'Silver Grey', 95.00, 5, 10),
('Fluorescent Neon Vinyl', 'Neon Green', 55.00, 30, 10),
('Heat Transfer Garment Vinyl', 'Navy Blue', 50.00, 60, 15),
('Frosted Window Etch Film', 'Translucent', 85.00, 18, 10),
('Holographic Prism Vinyl', 'Multi-color', 110.00, 12, 15);

-- Orders
INSERT INTO orders (order_ref, user_id, order_type, total_amount, status) VALUES
('ORD5824', 2, 'mobile', 450.00, 'Vinyl Cutting & Screen Prep'),
('ORD9624', 3, 'mobile', 675.00, 'Screen Printing'),
('ORD1001', 4, 'walk_in', 225.00, 'Ready for Collection'),
('ORD1002', 5, 'mobile', 550.00, 'Order Received & Verified'),
('ORD1003', 6, 'walk_in', 380.00, 'Completed'),
('ORD1004', 7, 'mobile', 750.00, 'Vinyl Cutting & Screen Prep'),
('ORD1005', 8, 'mobile', 300.00, 'Order Received & Verified'),
('ORD1006', 9, 'walk_in', 950.00, 'Screen Printing'),
('ORD1007', 10, 'mobile', 420.00, 'Ready for Collection'),
('ORD1008', 2, 'mobile', 660.00, 'Completed');

-- Order Items
INSERT INTO order_items (order_id, design_name, artwork_file_url, material_id, quantity, unit_price, subtotal) VALUES
(1, 'Glass Block Back Logo', 'uploads/glass_block.png', 1, 10, 45.00, 450.00),
(2, 'Club Rugby Crest Emblem', 'uploads/rugby_crest.svg', 3, 9, 75.00, 675.00),
(3, 'Storefront Window Lettering', 'uploads/storefront.png', 2, 5, 45.00, 225.00),
(4, 'Fleet Delivery Vehicle Decal', 'uploads/fleet_van.svg', 7, 10, 55.00, 550.00),
(5, 'Gym Member Hoodies Transfer', 'uploads/gym_hoodie.png', 8, 7, 50.00, 350.00),
(6, 'Safety Helmet Hazard Stripes', 'uploads/hazard_stripes.svg', 6, 8, 95.00, 760.00),
(7, 'Cafe Glass Door Opening Hours', 'uploads/cafe_hours.png', 9, 3, 85.00, 255.00),
(8, 'Custom Motorcycle Fairing Art', 'uploads/fairing_art.svg', 10, 8, 110.00, 880.00),
(9, 'Promotional Event Stickers', 'uploads/event_sticker.png', 4, 7, 60.00, 420.00),
(10, 'Varsity Jacket Chest Badges', 'uploads/varsity_badge.png', 5, 8, 75.00, 600.00);