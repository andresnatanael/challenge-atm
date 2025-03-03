---------------------------------------------------------------------------
-- 1) Clear existing data (optional)
---------------------------------------------------------------------------
TRUNCATE public."CardTransaction", public."Cards", public."Users" CASCADE;

---------------------------------------------------------------------------
-- 2) Create Users
---------------------------------------------------------------------------
INSERT INTO public."Users" ("Username", "DocumentNumber")
VALUES
    ('andres_soria',   'ID12345678'),
    ('pedro_mendoza',  'ID23456789'),
    ('leo_messi',      'ID34567890');

---------------------------------------------------------------------------
-- 3) Insert Cards
---------------------------------------------------------------------------
DO $$
    DECLARE
        andres_id INTEGER;
        pedro_id  INTEGER;
        leo_id    INTEGER;
    BEGIN
        SELECT "Id" INTO andres_id FROM public."Users" WHERE "Username" = 'andres_soria';
        SELECT "Id" INTO pedro_id  FROM public."Users" WHERE "Username" = 'pedro_mendoza';
        SELECT "Id" INTO leo_id    FROM public."Users" WHERE "Username" = 'leo_messi';

        /*
          Plaintext references:
            ANDRÉS:
              Card #1: 5218395232872386, PIN: 436826
              Card #2: 6550848153281573, PIN: 7185
            PEDRO:
              Card #1: 5249424854841018, PIN: 746288
            LEO:
              Card #1: 341551726417507,  PIN: 46068
        */

        INSERT INTO public."Cards"
        ("NumberHash",
         "PinHash",
         "Network",
         "Type",
         "UserId",
         "FailedLoginAttempts",
         "LockoutEnd",
         "Balance")
        VALUES
            -- ANDRÉS, Card #1
            (
                -- Encrypted with AES-256, e.g. 5218395232872386 -> 2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=
                '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=',
                -- PIN 436826 -> g29VgXigcROtLEBnTcy3nQ
                'g29VgXigcROtLEBnTcy3nQ==',
                1,  -- Example "Network"
                1,  -- Example "Type"
                andres_id,
                0,
                NULL,
                200000.00
            ),

            -- ANDRÉS, Card #2
            (
                -- 6550848153281573 -> U+ywCU246+C9IlyMwBlyP21TkxToN8xWlE5OZRud+SA=
                'U+ywCU246+C9IlyMwBlyP21TkxToN8xWlE5OZRud+SA=',
                -- PIN 7185 -> Lneftl0Rby2O/1Yxj5KNpQ==
                'Lneftl0Rby2O/1Yxj5KNpQ==',
                2,
                0,
                andres_id,
                0,
                NULL,
                50000.00
            ),

            -- PEDRO, Card #1
            (
                -- 5249424854841018 -> u3OjnrgduhMySOFQ5JxIKS7PyVMfhRjApQsNTcRUNBo=
                'u3OjnrgduhMySOFQ5JxIKS7PyVMfhRjApQsNTcRUNBo=',
                -- PIN 746288 -> nNjZRR6X5NlL/+B2PDtMQQ==
                'nNjZRR6X5NlL/+B2PDtMQQ==',
                1,
                1,
                pedro_id,
                0,
                NULL,
                68000.00
            ),

            -- LEO, Card #1
            (
                -- 341551726417507 -> 3PlRxXUfGDY2d+t3Bqx7cw==
                '3PlRxXUfGDY2d+t3Bqx7cw==',
                -- PIN 46068 -> PyhdzRQ2PpDQbKfqkRgiEg==
                'PyhdzRQ2PpDQbKfqkRgiEg==',
                2,
                0,
                leo_id,
                0,
                NULL,
                92000.00
            );
    END $$;

---------------------------------------------------------------------------
-- 4) Insert 40 Transactions for ANDRÉS's Card #1 ONLY
--    Card #1 (Andrés) encrypted "NumberHash" = 2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=
---------------------------------------------------------------------------
INSERT INTO public."CardTransaction"
("Amount", "Date", "CardNumberHash", "AtmLocation", "Type")
VALUES
    /*
      We'll do 30 deposits (Type=0) of 6,000 each = 180,000 total
      and 10 withdrawals (Type=1) of 3,000 each = 30,000 total
      Net change = +150,000
      Starting "conceptual" balance ~50,000 => Final = 200,000
    */

    -- 10 WITHDRAWALS (3,000 each; Type=1)
    (3000.00, '2025-01-01 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-02 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-03 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-04 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-05 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-06 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-07 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-08 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-09 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),
    (3000.00, '2025-01-10 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1),

    -- 30 DEPOSITS (6,000 each; Type=0)
    (6000.00, '2025-01-11 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Online Banking', 0),
    (6000.00, '2025-01-12 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Online Banking', 0),
    (6000.00, '2025-01-13 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0),
    (6000.00, '2025-01-14 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0),
    (6000.00, '2025-01-15 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0),
    (6000.00, '2025-01-16 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0),
    (6000.00, '2025-01-17 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0),
    (6000.00, '2025-01-18 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0),
    (6000.00, '2025-01-19 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0),
    (6000.00, '2025-01-20 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0),
    (6000.00, '2025-01-21 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0),
    (6000.00, '2025-01-22 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0),
    (6000.00, '2025-01-23 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0),
    (6000.00, '2025-01-24 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0),
    (6000.00, '2025-01-25 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0),
    (6000.00, '2025-01-26 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Shopping Center ATM', 0),
    (6000.00, '2025-01-27 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Shopping Center ATM', 0),
    (6000.00, '2025-01-28 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0),
    (6000.00, '2025-01-29 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0),
    (6000.00, '2025-01-30 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0),
    (6000.00, '2025-01-31 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0),
    (6000.00, '2025-02-01 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0),
    (6000.00, '2025-02-02 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0),
    (6000.00, '2025-02-03 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0),
    (6000.00, '2025-02-04 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0),
    (6000.00, '2025-02-05 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0),
    (6000.00, '2025-02-06 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0),
    (6000.00, '2025-02-07 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0),
    (6000.00, '2025-02-08 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0),
    (6000.00, '2025-02-09 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0),
    (6000.00, '2025-02-10 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0);