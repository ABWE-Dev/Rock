﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 8, "1.0.14" )]
    public class BaptismIsDeleted : Migration
    {
        public override void Up()
        {
            Sql( @"
            IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'IsDeleted' AND Object_ID = Object_ID(N'[_com_centralaz_Baptism_Baptizee]'))
BEGIN
            ALTER TABLE [_com_centralaz_Baptism_Baptizee]
            ADD IsDeleted BIT
END
            " );
            //Adding the value for the Linked Page Attribute "Blackout Day Page" on the BaptismCampusDetail block
            RockMigrationHelper.AddBlockAttributeValue( "94256870-5A3A-4BAE-A489-6BA9F94C0ED0", "CB638267-0065-4FFC-A665-BCE475BD022D", @"a3882eae-f086-467b-9f3c-dc0db75403f7" );
        }

        public override void Down()
        {

        }
    }
}