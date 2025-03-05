---------------------------------------------------------------------------
-- 1) Clear existing data (optional)
---------------------------------------------------------------------------
TRUNCATE public."CardTransactions", public."Cards", public."Users" CASCADE;

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
                -- PIN 436826 -> g29VgXigcROtLEBnTcy3nQ==
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
INSERT INTO public."CardTransactions"
("Amount", "Date", "CardNumberHash", "AtmLocation", "Type", "IdempotencyKey")
VALUES
    /*
      We'll do 30 deposits (Type=0) of 6,000 each = 180,000 total
      and 10 withdrawals (Type=1) of 3,000 each = 30,000 total
      Net change = +150,000
      Starting "conceptual" balance ~50,000 => Final = 200,000
    */

    -- 10 WITHDRAWALS (3,000 each; Type=1)
    (3000.00, '2025-01-01 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'a1b2c3d4-e5f6-4101-8d9e-10f11g12h13i'),
    (3000.00, '2025-01-02 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'b2c3d4e5-f6g7-4102-9e0f-11g12h13i14j'),
    (3000.00, '2025-01-03 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'c3d4e5f6-g7h8-4103-0f1g-12h13i14j15k'),
    (3000.00, '2025-01-04 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'd4e5f6g7-h8i9-4104-1g2h-13i14j15k16l'),
    (3000.00, '2025-01-05 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'e5f6g7h8-i9j0-4105-2h3i-14j15k16l17m'),
    (3000.00, '2025-01-06 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'f6g7h8i9-j0k1-4106-3i4j-15k16l17m18n'),
    (3000.00, '2025-01-07 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'g7h8i9j0-k1l2-4107-4j5k-16l17m18n19o'),
    (3000.00, '2025-01-08 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'h8i9j0k1-l2m3-4108-5k6l-17m18n19o20p'),
    (3000.00, '2025-01-09 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'i9j0k1l2-m3n4-4109-6l7m-18n19o20p21q'),
    (3000.00, '2025-01-10 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'ATM - Downtown', 1, 'j0k1l2m3-n4o5-4110-7m8n-19o20p21q22r'),

    -- 30 DEPOSITS (6,000 each; Type=0)
    (6000.00, '2025-01-11 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Online Banking', 0, 'k1l2m3n4-o5p6-4111-8n9o-20p21q22r23s'),
    (6000.00, '2025-01-12 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Online Banking', 0, 'l2m3n4o5-p6q7-4112-9o0p-21q22r23s24t'),
    (6000.00, '2025-01-13 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0, 'm3n4o5p6-q7r8-4113-0p1q-22r23s24t25u'),
    (6000.00, '2025-01-14 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0, 'n4o5p6q7-r8s9-4114-1q2r-23s24t25u26v'),
    (6000.00, '2025-01-15 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0, 'o5p6q7r8-s9t0-4115-2r3s-24t25u26v27w'),
    (6000.00, '2025-01-16 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0, 'p6q7r8s9-t0u1-4116-3s4t-25u26v27w28x'),
    (6000.00, '2025-01-17 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Main St Branch', 0, 'q7r8s9t0-u1v2-4117-4t5u-26v27w28x29y'),
    (6000.00, '2025-01-18 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0, 'r8s9t0u1-v2w3-4118-5u6v-27w28x29y30z'),
    (6000.00, '2025-01-19 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0, 's9t0u1v2-w3x4-4119-6v7w-28x29y30z31a'),
    (6000.00, '2025-01-20 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0, 't0u1v2w3-x4y5-4120-7w8x-29y30z31a32b'),
    (6000.00, '2025-01-21 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Mobile Banking', 0, 'u1v2w3x4-y5z6-4121-8x9y-30z31a32b33c'),
    (6000.00, '2025-01-22 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0, 'v2w3x4y5-z6a7-4122-9y0z-31a32b33c34d'),
    (6000.00, '2025-01-23 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0, 'w3x4y5z6-a7b8-4123-0z1a-32b33c34d35e'),
    (6000.00, '2025-01-24 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0, 'x4y5z6a7-b8c9-4124-1a2b-33c34d35e36f'),
    (6000.00, '2025-01-25 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Airport ATM', 0, 'y5z6a7b8-c9d0-4125-2b3c-34d35e36f37g'),
    (6000.00, '2025-01-26 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Shopping Center ATM', 0, 'z6a7b8c9-d0e1-4126-3c4d-35e36f37g38h'),
    (6000.00, '2025-01-27 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Shopping Center ATM', 0, 'a7b8c9d0-e1f2-4127-4d5e-36f37g38h39i'),
    (6000.00, '2025-01-28 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0, 'b8c9d0e1-f2g3-4128-5e6f-37g38h39i40j'),
    (6000.00, '2025-01-29 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0, 'c9d0e1f2-g3h4-4129-6f7g-38h39i40j41k'),
    (6000.00, '2025-01-30 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0, 'd0e1f2g3-h4i5-4130-7g8h-39i40j41k42l'),
    (6000.00, '2025-01-31 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'West End Branch', 0, 'e1f2g3h4-i5j6-4131-8h9i-40j41k42l43m'),
    (6000.00, '2025-02-01 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0, 'f2g3h4i5-j6k7-4201-9i0j-41k42l43m44n'),
    (6000.00, '2025-02-02 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0, 'g3h4i5j6-k7l8-4202-0j1k-42l43m44n45o'),
    (6000.00, '2025-02-03 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0, 'h4i5j6k7-l8m9-4203-1k2l-43m44n45o46p'),
    (6000.00, '2025-02-04 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'East Side Branch', 0, 'i5j6k7l8-m9n0-4204-2l3m-44n45o46p47q'),
    (6000.00, '2025-02-05 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0, 'j6k7l8m9-n0o1-4205-3m4n-45o46p47q48r'),
    (6000.00, '2025-02-06 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0, 'k7l8m9n0-o1p2-4206-4n5o-46p47q48r49s'),
    (6000.00, '2025-02-07 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0, 'l8m9n0o1-p2q3-4207-5o6p-47q48r49s50t'),
    (6000.00, '2025-02-08 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0, 'm9n0o1p2-q3r4-4208-6p7q-48r49s50t51u'),
    (6000.00, '2025-02-09 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0, 'n0o1p2q3-r4s5-4209-7q8r-49s50t51u52v'),
    (6000.00, '2025-02-10 09:00:00+00', '2sbRKHjrtPNhMXn0FTxQEM3F110HUzCOB/bkezXPHWk=', 'Downtown Branch', 0, 'o1p2q3r4-s5t6-4210-8r9s-50t51u52v53w');