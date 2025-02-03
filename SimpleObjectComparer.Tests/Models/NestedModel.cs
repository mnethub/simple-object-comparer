using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleObjectComparer.Tests.Models
{
    internal class NestedModel
    {
        public class ContactInfo
        {
            public string Email { get; set; }
            public string Phone { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public ContactInfo Contact { get; set; }
        }

        public class Job
        {
            public string Title { get; set; }
            public decimal Salary { get; set; }
            public Address WorkAddress { get; set; }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public Address HomeAddress { get; set; }
            public List<Job> JobHistory { get; set; }
        }

    }
}
