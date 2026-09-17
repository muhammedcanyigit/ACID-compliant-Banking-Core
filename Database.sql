CREATE TABLE "Customer" (
    
"Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "FirstName" VARCHAR(50) NOT NULL,
    "LastName" VARCHAR(50) NOT NULL,
    "Email" VARCHAR(100) UNIQUE NOT NULL,
    "IdentityNumber" VARCHAR(11) UNIQUE NOT NULL, -- TC Kimlik No / Pasaport
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(20) NOT NULL DEFAULT 'Customer', -- Customer, Teller, FraudAnalyst
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE "Accounts" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
   "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CustomerId" UUID NOT NULL,
    "AccountNumber" VARCHAR(20) UNIQUE NOT NULL, -- IBAN / Hesap No
    "Balance" DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'TRY', -- TRY, USD, EUR
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "FK_Accounts_Customer" FOREIGN KEY ("CustomerId") REFERENCES "Customer"("Id") ON DELETE RESTRICT,

    CONSTRAINT "CHK_Accounts_Balance" CHECK ("Balance" >= 0)
);



CREATE TABLE "Transactions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "SenderAccountId" UUID NOT NULL,
    "ReceiverAccountId" UUID NOT NULL,
    "Amount" DECIMAL(18, 2) NOT NULL,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Failed, Flagged
    "Description" VARCHAR(250),
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Yabancı Anahtarlar
    CONSTRAINT "FK_Transactions_SenderAccount" FOREIGN KEY ("SenderAccountId") 
        REFERENCES "Accounts"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Transactions_ReceiverAccount" FOREIGN KEY ("ReceiverAccountId") 
        REFERENCES "Accounts"("Id") ON DELETE RESTRICT,
        
    -- BANKACILIK KISITI: Transfer tutarı 0 veya eksi olamaz!
    CONSTRAINT "CHK_Transaction_Amount_Positive" CHECK ("Amount" > 0)
    );


    CREATE INDEX "IX_Accounts_CustomerId" ON "Accounts" ("CustomerId");
    CREATE INDEX "IX_Transactions_SenderAccountId" ON "Transactions" ("SenderAccountId");
    CREATE INDEX "IX_Transactions_ReceiverAccountId" ON "Transactions" ("ReceiverAccountId");
    CREATE INDEX "IX_Transactions_CreatedAt" ON "Transactions" ("CreatedAt");
