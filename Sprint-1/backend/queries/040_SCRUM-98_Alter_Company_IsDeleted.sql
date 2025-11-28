use Fiesta_Fries_DB;
go

ALTER TABLE empresa ADD IsDeleted BIT NOT NULL DEFAULT 0;