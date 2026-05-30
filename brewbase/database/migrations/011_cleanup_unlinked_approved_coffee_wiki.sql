-- Cleanup legacy standalone approved coffee wiki articles (dev/seed canonical model).
-- Replaces unlinked approved module=coffee rows with linked wikis for catalog coffees 1 and 2.
-- Requires matching rows in coffee (see seed_init.sql). Skips insert when coffee row is missing.

DELETE FROM report
WHERE article_id IN (
    SELECT id
    FROM article
    WHERE module = 'coffee'
      AND status = 'Approved'
      AND coffee_id IS NULL
);

DELETE FROM article
WHERE module = 'coffee'
  AND status = 'Approved'
  AND coffee_id IS NULL;

INSERT INTO article (
    title,
    content,
    status,
    module,
    created_at,
    updated_at,
    published_at,
    moderated_by_user_id,
    moderated_at,
    moderation_comment,
    user_id,
    coffee_id
)
SELECT
    'Etiopia Guji Natural',
    $$Kraj pochodzenia ziaren: Etiopia
Odmiana: Heirloom
Obróbka ziaren: Natural
Profil smakowy: Jagody, Kwiaty, Czekolada

Naturalnie obrabiana kawa z regionu Guji. W filiżance dominują nuty dojrzałych jagód i delikatnej kwiatowości, z aksamitnym body typowym dla etiopskich naturali.$$,
    'Approved',
    'coffee',
    '2026-04-10 09:15:00',
    '2026-04-12 11:00:00',
    '2026-04-12 11:00:00',
    2,
    '2026-04-12 11:00:00',
    NULL,
    1,
    1
WHERE NOT EXISTS (
    SELECT 1
    FROM article existing
    WHERE existing.module = 'coffee'
      AND existing.status = 'Approved'
      AND existing.coffee_id = 1
)
AND EXISTS (SELECT 1 FROM coffee WHERE id = 1);

INSERT INTO article (
    title,
    content,
    status,
    module,
    created_at,
    updated_at,
    published_at,
    moderated_by_user_id,
    moderated_at,
    moderation_comment,
    user_id,
    coffee_id
)
SELECT
    'Kolumbia Huila Washed',
    $$Kraj pochodzenia ziaren: Kolumbia
Odmiana: Caturra
Obróbka ziaren: Washed
Profil smakowy: Czekolada, Karmel, Orzechy

Klasyczna washed z Huili o zbalansowanym profilu: czekolada, karmel i łagodna orzechowa słodycz. Dobrze sprawdza się jako przelew i espresso.$$,
    'Approved',
    'coffee',
    '2026-04-14 08:00:00',
    '2026-04-15 16:45:00',
    '2026-04-15 16:45:00',
    2,
    '2026-04-15 16:45:00',
    NULL,
    1,
    2
WHERE NOT EXISTS (
    SELECT 1
    FROM article existing
    WHERE existing.module = 'coffee'
      AND existing.status = 'Approved'
      AND existing.coffee_id = 2
)
AND EXISTS (SELECT 1 FROM coffee WHERE id = 2);
