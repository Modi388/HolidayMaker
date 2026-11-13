-- Holiday Maker: alla SQL-frågor som programmet använder.
-- Skrivna och testade i DataGrip. :namn är parametrar som fylls i vid körning
-- (i C#-koden heter de @namn och fylls i med AddWithValue).

-- 1. Registrera kund
INSERT INTO customer (name, email, phone_number)
VALUES (:name, :email, :phone);

-- 2. Söka/välja kund (del av namnet)
SELECT id, name, email, phone_number
FROM customer
WHERE name LIKE '%' || :search || '%'
ORDER BY name;

-- 3. Söka lediga rum
-- Ett rum är upptaget om en bokning börjar före utcheckningen OCH slutar efter incheckningen.
-- Filter: 0 eller NULL = inget filter.
SELECT a.name, a.city, r.room_number, r.max_guests, r.price_per_night, a.rating, r.id
FROM room r
JOIN accommodation a ON a.id = r.accommodation_id
JOIN facility f      ON f.accommodation_id = a.id
WHERE a.city = :city
  AND r.max_guests >= :guests
  AND NOT EXISTS (
      SELECT 1 FROM booking b
      WHERE b.room_id = r.id
        AND b.start_date < :check_out
        AND b.end_date   > :check_in)
  AND (:need_pool = 0 OR f.has_pool = 1)
  AND (:need_restaurant = 0 OR f.has_restaurant = 1)
  AND (:max_beach IS NULL OR a.distance_to_beach_m <= :max_beach)
  AND (:max_center IS NULL OR a.distance_to_center_m <= :max_center)
ORDER BY r.price_per_night ASC;   -- alternativ sortering: a.rating DESC

-- 4. Skapa bokning (läggs bara in om rummet är ledigt och stort nog: 1 rad = ok, 0 rader = nekad)
INSERT INTO booking (customer_id, room_id, start_date, end_date, number_of_guests)
SELECT :customer_id, :room_id, :check_in, :check_out, :guests
WHERE :guests <= (SELECT max_guests FROM room WHERE id = :room_id)
  AND NOT EXISTS (
      SELECT 1 FROM booking
      WHERE room_id = :room_id
        AND start_date < :check_out
        AND end_date   > :check_in);

-- 5. Visa kundens bokningar med totalpris (antal nätter × pris per natt)
SELECT b.id, a.name, r.room_number, b.start_date, b.end_date, b.number_of_guests,
       CAST(julianday(b.end_date) - julianday(b.start_date) AS INTEGER) AS nights,
       CAST(julianday(b.end_date) - julianday(b.start_date) AS INTEGER) * r.price_per_night AS total_price
FROM booking b
JOIN room r          ON r.id = b.room_id
JOIN accommodation a ON a.id = r.accommodation_id
WHERE b.customer_id = :customer_id
ORDER BY b.start_date;

-- 6. Ändra antal personer (bara om det ryms i rummet)
UPDATE booking
SET number_of_guests = :guests
WHERE id = :booking_id
  AND customer_id = :customer_id
  AND :guests <= (SELECT max_guests FROM room WHERE room.id = booking.room_id);

-- 7. Avboka
DELETE FROM booking
WHERE id = :booking_id
  AND customer_id = :customer_id;
