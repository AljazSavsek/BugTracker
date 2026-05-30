-- ============================================================
--  BugTracker – MySQL shema + vzorčni podatki
--  Privzeti admin:  username=admin  password=Admin123!
-- ============================================================
CREATE DATABASE IF NOT EXISTS bugtracker
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE bugtracker;

CREATE TABLE IF NOT EXISTS UPORABNIKI (
    id_uporabnika   INT          NOT NULL AUTO_INCREMENT,
    uporabnisko_ime VARCHAR(50)  NOT NULL UNIQUE,
    geslo           VARCHAR(255) NOT NULL,
    ime             VARCHAR(50)  NOT NULL,
    priimek         VARCHAR(50)  NOT NULL,
    email           VARCHAR(100) NOT NULL UNIQUE,
    vloga           VARCHAR(20)  NOT NULL DEFAULT 'Developer',
    aktiven         TINYINT(1)   NOT NULL DEFAULT 1,
    PRIMARY KEY (id_uporabnika)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KATEGORIJE (
    id_kategorije INT         NOT NULL AUTO_INCREMENT,
    naziv         VARCHAR(50) NOT NULL UNIQUE,
    opis          TEXT,
    PRIMARY KEY (id_kategorije)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS NAPAKE (
    id_napake         INT          NOT NULL AUTO_INCREMENT,
    naslov            VARCHAR(200) NOT NULL,
    opis              TEXT,
    status            VARCHAR(20)  NOT NULL DEFAULT 'Odprt',
    prioriteta        VARCHAR(20)  NOT NULL DEFAULT 'Srednja',
    id_kategorije     INT,
    id_ustvaritelja   INT          NOT NULL,
    id_dodeljenega    INT,
    datum_ustvarjeno  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    datum_spremenjeno DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id_napake),
    FOREIGN KEY (id_kategorije)   REFERENCES KATEGORIJE(id_kategorije) ON DELETE SET NULL,
    FOREIGN KEY (id_ustvaritelja) REFERENCES UPORABNIKI(id_uporabnika)  ON DELETE RESTRICT,
    FOREIGN KEY (id_dodeljenega)  REFERENCES UPORABNIKI(id_uporabnika)  ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KOMENTARJI (
    id_komentarja INT      NOT NULL AUTO_INCREMENT,
    id_napake     INT      NOT NULL,
    id_uporabnika INT      NOT NULL,
    vsebina       TEXT     NOT NULL,
    datum_cas     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_komentarja),
    FOREIGN KEY (id_napake)     REFERENCES NAPAKE(id_napake)        ON DELETE CASCADE,
    FOREIGN KEY (id_uporabnika) REFERENCES UPORABNIKI(id_uporabnika) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ZGODOVINA (
    id_zgodovine   INT         NOT NULL AUTO_INCREMENT,
    id_napake      INT         NOT NULL,
    id_uporabnika  INT         NOT NULL,
    polje          VARCHAR(50) NOT NULL,
    stara_vrednost TEXT,
    nova_vrednost  TEXT,
    datum_cas      DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_zgodovine),
    FOREIGN KEY (id_napake)     REFERENCES NAPAKE(id_napake)        ON DELETE CASCADE,
    FOREIGN KEY (id_uporabnika) REFERENCES UPORABNIKI(id_uporabnika) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ── Vzorčni podatki ────────────────────────────────────────────
-- Geslo za vse: Admin123!
INSERT INTO UPORABNIKI (uporabnisko_ime, geslo, ime, priimek, email, vloga, aktiven) VALUES
('admin',   '$2b$11$e4W3BBdVWlu8eX6ZpU6Mv.rNV2ix8.bDCeBGW7FaG.CXk8idhBsZO','Admin','Sistemski','admin@bugtracker.si',  'Admin',    1),
('jnovak',  '$2b$11$e4W3BBdVWlu8eX6ZpU6Mv.rNV2ix8.bDCeBGW7FaG.CXk8idhBsZO','Jana', 'Novak',    'j.novak@bugtracker.si','Developer',1),
('mkovac',  '$2b$11$e4W3BBdVWlu8eX6ZpU6Mv.rNV2ix8.bDCeBGW7FaG.CXk8idhBsZO','Miha', 'Kovač',    'm.kovac@bugtracker.si','Tester',   1),
('ahorvat', '$2b$11$e4W3BBdVWlu8eX6ZpU6Mv.rNV2ix8.bDCeBGW7FaG.CXk8idhBsZO','Ana',  'Horvat',   'a.horvat@bugtracker.si','Developer',0),
('rsitar',  '$2b$11$e4W3BBdVWlu8eX6ZpU6Mv.rNV2ix8.bDCeBGW7FaG.CXk8idhBsZO','Rok',  'Sitar',    'r.sitar@bugtracker.si','Tester',   1);

INSERT INTO KATEGORIJE (naziv, opis) VALUES
('UI',          'Napake v uporabniškem vmesniku'),
('Backend',     'Napake v strežniški logiki'),
('Baza',        'Napake v podatkovni bazi'),
('Varnost',     'Varnostne ranljivosti'),
('Zmogljivost', 'Napake v zmogljivosti');

INSERT INTO NAPAKE (naslov, opis, status, prioriteta, id_kategorije, id_ustvaritelja, id_dodeljenega) VALUES
('Napaka pri prijavi z Google računom', 'OAuth2 vrne 401.',                  'Odprt',  'Visoka',   4, 1, 2),
('CSS prelom na mobilnih napravah',     'Meni se prelomi pod 480px.',        'V delu', 'Srednja',  1, 3, 2),
('Napačen izračun popustov',            'Popust se ne odšteje v košarici.',  'Rešen',  'Kritična', 2, 2, 2),
('Počasno nalaganje admin panela',      'Stran se nalaga > 5 sekund.',       'Odprt',  'Nizka',    5, 3, 5),
('SQL injection v iskanju',             'Iskalno polje ni zaščiteno.',       'V delu', 'Kritična', 4, 1, 2),
('Export v Excel ne deluje',            'Gumb vrže NullReferenceException.', 'Zaprt',  'Srednja',  1, 2, 3),
('Napaka 500 pri brisanju komentarja',  'DELETE /api/comment vrne 500.',     'Odprt',  'Visoka',   2, 3, 2),
('Seja poteče prezgodaj',               'Seja poteče po 5 min.',            'V delu', 'Srednja',  4, 1, 5);

INSERT INTO ZGODOVINA (id_napake, id_uporabnika, polje, stara_vrednost, nova_vrednost) VALUES
(3, 2, 'status',    'Odprt',  'V delu'),
(3, 2, 'status',    'V delu', 'Rešen'),
(3, 1, 'prioriteta','Visoka', 'Kritična'),
(5, 1, 'status',    'Odprt',  'V delu'),
(6, 3, 'status',    'Odprt',  'Zaprt');
