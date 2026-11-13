PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS booking;
DROP TABLE IF EXISTS room;
DROP TABLE IF EXISTS facility;
DROP TABLE IF EXISTS accommodation;
DROP TABLE IF EXISTS customer;

CREATE TABLE customer (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    phone_number TEXT NOT NULL
);

CREATE TABLE accommodation (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    city TEXT NOT NULL,
    type TEXT NOT NULL,
    distance_to_beach_m INTEGER NOT NULL CHECK (distance_to_beach_m >= 0),
    distance_to_center_m INTEGER NOT NULL CHECK (distance_to_center_m >= 0),
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5)
);

CREATE TABLE facility (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    accommodation_id INTEGER NOT NULL UNIQUE,
    has_pool INTEGER NOT NULL DEFAULT 0 CHECK (has_pool IN (0, 1)),
    has_restaurant INTEGER NOT NULL DEFAULT 0 CHECK (has_restaurant IN (0, 1)),
    FOREIGN KEY (accommodation_id) REFERENCES accommodation(id)
);

CREATE TABLE room (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    accommodation_id INTEGER NOT NULL,
    room_number TEXT NOT NULL,
    max_guests INTEGER NOT NULL CHECK (max_guests > 0),
    price_per_night REAL NOT NULL CHECK (price_per_night > 0),
    UNIQUE (accommodation_id, room_number),
    FOREIGN KEY (accommodation_id) REFERENCES accommodation(id)
);

CREATE TABLE booking (
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	customer_id INTEGER NOT NULL,
	room_id INTEGER NOT NULL,
	start_date DATE NOT NULL,
	end_date DATE NOT NULL CHECK (end_date > start_date),
	number_of_guests INTEGER NOT NULL CHECK (number_of_guests > 0),
	FOREIGN KEY (customer_id) REFERENCES customer(id),
	FOREIGN KEY (room_id) REFERENCES room(id)
	);