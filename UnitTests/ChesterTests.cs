using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedLibs.Chester;
using SharedLibs.Migros;
using SharedLibs.Model;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class ChesterTests
    {
        private const String username = "migros_hpsm@norply.xerox.com";
        private const String password = "qldC0V6b";
        private String cookies;
        private EquipmentWso theEquipment;
        private CustomerWso theCustomer;
        private MigrosData migros;

        public ChesterTests()
        {
            Security security = new Security("https://chester.xerox.com.tr/WebServices/Security.asmx");
            bool result = security.Login(username, password);
            Assert.IsTrue(result, "could not login");
            cookies = security.Cookies;

            migros = new MigrosData { 
                user = "",
                password = "",
                Cihaz_Seri_No = "1124286642",
                Magaza_Telefonu = "09005335353",
                Cagri_Aciklamasi = "sample çağrı açıklaması",
                Cagri_Ilgilisi = "KEREM EKE 14538",
                Magaza_Kodu = "13A",
                Migros_Cagri_No = "SD12579",
                Cagri_Durum_Kodu = "Atandi"
            };
        }

        [TestMethod]
        public void TestGetCustomer()
        {
            CustomerService customerService = new CustomerService();
            customerService.Cookies = cookies;

            CustomerWso customer = customerService.GetCustomer();
            Assert.IsNotNull(customer);

            theCustomer = customer;
        }

        [TestMethod]
        public void TestGetEquipments() {

            CustomerService customerService = new CustomerService();
            customerService.Cookies = cookies;

            EquipmentWso[] equipments = customerService.GetEquipments(null, "");
            Assert.AreNotEqual(0, equipments.Length, "there are no equipments");
        }

        [TestMethod]
        public void TestGetEquipment()
        {

            CustomerService customerService = new CustomerService();
            customerService.Cookies = cookies;

            EquipmentWso[] equipments = customerService.GetEquipments(null, "3900489985");
            Assert.AreEqual(1, equipments.Length, "could not find the equipment");

            theEquipment = equipments[0];
        }

        [TestMethod]
        public void TestGetServiceCalls() {

            CustomerService customerService = new CustomerService();
            customerService.Cookies = cookies;

            ServiceCallWso[] serviceCalls = customerService.GetServiceCalls(null, "301291560", "", null, "", null, null, null, null);
            Assert.IsNotNull(serviceCalls);
        }

        [TestMethod]
        public void TestCreateCustomerContactPerson() {

            Common commonService = new Common();
            commonService.Cookies = cookies;

            TestGetCustomer();

            String name = "sample contact person";
            String email = "kadirmalak@gmail.com"; // XXX: migrostan gelmiyor
            String phone = "09009009090"; // XXX: migrostan gelmiyor

            CreatedCustomerContactPersonWso createdCustomer = commonService.CreateCustomerContactPerson(
                theCustomer.CustomerNumber, theCustomer.CustomerID, name, email, phone);
            Assert.IsNotNull(createdCustomer);
        }

        [TestMethod]
        public void TestCreateCustomerAddress()
        {

            Common commonService = new Common();
            commonService.Cookies = cookies;

            TestGetCustomer();

            // XXX: migrostan makine adresi ile ilgili bilgiler gelmiyor
            String name = "DVS";
            String street = "ömer avni mh. sulak çeşme sk.";
            String city = "istanbul";
            String zipCode = "34427";
            String country = "TR";
            String buildingCode = "3";
            String floor = "3";
            String roomNumber = "6";

            CreatedCustomerAddressWso createdAddress = commonService.CreateCustomerAddress(
                theCustomer.CustomerNumber, theCustomer.CustomerID, name, 
                street, city, zipCode, country, buildingCode, floor, roomNumber);
            Assert.IsNotNull(createdAddress);
        }

        [TestMethod]
        public void TestCreateServiceCall() {

            CustomerService customerService = new CustomerService();
            customerService.Cookies = cookies;

            TestGetCustomer();
            TestGetEquipment();

            String[] contactPersonParts = migros.Cagri_Ilgilisi.Split(new char[] { ' ' });
            List<String> list = new List<String>(contactPersonParts);
            int lastIndex = list.Count - 1;
            String contactPersonIDStr = list[lastIndex];
            list.RemoveAt(lastIndex);
            String contactPersonName = String.Join(" ", list.ToArray());
            int contactPersonID = int.Parse(contactPersonIDStr);

            int equipmentID = theEquipment.EquipmentID;
            String equipmentSerialNumber = migros.Cihaz_Seri_No;
            String title = migros.Magaza_Telefonu + ", " + migros.Cagri_Aciklamasi;
            String description = migros.Cagri_Aciklamasi + "\n" +
                "İlgili: " + contactPersonName + "\n" +
                "Tel: " + migros.Magaza_Telefonu + "\n" +
                "M: " + migros.Magaza_Kodu;
            int callTypeID = 17;
            String customerReference = migros.Migros_Cagri_No;
            int addressID = theEquipment.AddressID;

            CreatedServiceCallWso createdServiceCall = customerService.CreateServiceCall(
                equipmentID, equipmentSerialNumber, title, description, 
                callTypeID, customerReference, addressID, contactPersonID);
            Assert.IsNotNull(createdServiceCall);
        }
    }
}
