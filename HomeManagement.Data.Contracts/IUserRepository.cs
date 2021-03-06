﻿using HomeManagement.Contracts.Repositories;
using HomeManagement.Domain;

namespace HomeManagement.Data
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByEmail(string email);
    }
}
