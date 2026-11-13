PRAGMA foreign_keys = ON;

-- Boenden (5 st, 3 orter)
INSERT INTO accommodation (id, name, city, type, distance_to_beach_m, distance_to_center_m, rating) VALUES
(1, 'Hotel Strandbris',     'Ystad',  'Hotel',   150,  900, 4),
(2, 'Pensionat Måsen',      'Ystad',  'Pension', 600,  300, 3),
(3, 'Båstad Seaside Hotel', 'Båstad', 'Hotel',    80, 1500, 5),
(4, 'Villa Ljung',          'Båstad', 'Pension',1200,  400, 2),
(5, 'Hotell Centralen',     'Malmö',  'Hotel',  3000,  100, 4);

-- Faciliteter (1 rad per boende)
INSERT INTO facility (accommodation_id, has_pool, has_restaurant) VALUES
(1, 1, 1),
(2, 0, 0),
(3, 1, 1),
(4, 1, 0),
(5, 0, 1);

-- Rum (20 st, kapacitet 1, 2, 4 och 6)
INSERT INTO room (id, accommodation_id, room_number, max_guests, price_per_night) VALUES
(1,  1, '101', 2, 1200), (2,  1, '102', 2, 1350), (3,  1, '103', 4, 2100), (4,  1, '104', 1,  850),
(5,  2, '1',   1,  600), (6,  2, '2',   2,  750), (7,  2, '3',   2,  800), (8,  2, '4',   4, 1100),
(9,  3, '201', 2, 1900), (10, 3, '202', 2, 2000), (11, 3, '203', 4, 3200), (12, 3, '204', 6, 4100),
(13, 4, 'A',   2,  950), (14, 4, 'B',   4, 1400), (15, 4, 'C',   1,  700),
(16, 5, '301', 1,  990), (17, 5, '302', 1,  990), (18, 5, '303', 2, 1290), (19, 5, '304', 2, 1390), (20, 5, '305', 4, 1990);

-- Kunder (5 fiktiva)
INSERT INTO customer (name, email, phone_number) VALUES
('Tyrion Lannister',       'Tyrion.Lannister@example.com',       '070-111 11 11'),
('Tywin Lannister',        'Tywin.Lannister@example.com',        '070-222 22 22'),
('Tony Soprano',           'Tony.Soprano@example.com',           '070-333 33 33'),
('Christopher Moltisanti', 'Christopher.Moltisanti@example.com', '070-444 44 44'),
('Bruce Wayne',            'Bruce.Wayne@example.com',            '070-555 55 55');

-- Bokningar (25 st, december 2026)
INSERT INTO booking (customer_id, room_id, start_date, end_date, number_of_guests) VALUES
(1, 1,  '2026-12-10', '2026-12-13', 2),
(2, 1,  '2026-12-20', '2026-12-24', 2),
(3, 2,  '2026-12-05', '2026-12-08', 1),
(4, 3,  '2026-12-12', '2026-12-19', 4),
(5, 4,  '2026-12-01', '2026-12-03', 1),
(1, 5,  '2026-12-15', '2026-12-16', 1),
(2, 6,  '2026-12-03', '2026-12-10', 2),
(3, 7,  '2026-12-18', '2026-12-22', 2),
(4, 8,  '2026-12-26', '2026-12-31', 3),
(5, 9,  '2026-12-07', '2026-12-11', 2),
(1, 10, '2026-12-14', '2026-12-21', 2),
(2, 11, '2026-12-01', '2026-12-06', 3),
(3, 12, '2026-12-27', '2026-12-31', 6),
(4, 13, '2026-12-09', '2026-12-12', 2),
(5, 14, '2026-12-19', '2026-12-26', 4),
(1, 15, '2026-12-02', '2026-12-04', 1),
(2, 16, '2026-12-23', '2026-12-27', 1),
(3, 17, '2026-12-23', '2026-12-27', 1),
(4, 18, '2026-12-23', '2026-12-27', 2),
(5, 19, '2026-12-23', '2026-12-27', 2),
(1, 20, '2026-12-23', '2026-12-27', 4),
(2, 16, '2026-12-05', '2026-12-07', 1),
(3, 18, '2026-12-11', '2026-12-14', 2),
(4, 9,  '2026-12-11', '2026-12-14', 2),
(5, 3,  '2026-12-28', '2026-12-30', 3);
