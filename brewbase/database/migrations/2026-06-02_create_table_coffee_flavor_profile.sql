CREATE TABLE coffee_flavor_profile
(
    coffee_id INT NOT NULL,
    flavor_profile_id INT NOT NULL,
    CONSTRAINT pk_coffee_flavor_profile PRIMARY KEY (coffee_id, flavor_profile_id),
    CONSTRAINT fk_coffee_flavor_profile_coffee FOREIGN KEY (coffee_id) REFERENCES coffee(id) ON DELETE CASCADE,
    CONSTRAINT fk_coffee_flavor_profile_flavor_profile  FOREIGN KEY (flavor_profile_id) REFERENCES flavor_profile(id) ON DELETE CASCADE
);
