﻿using HomeManagement.Business.Contracts;
using HomeManagement.Data;
using HomeManagement.Mapper;
using HomeManagement.Models;
using System.Collections.Generic;
using System.Linq;

namespace HomeManagement.Business.Units
{
    public class MonthlyExpenseService : IMonthlyExpenseService
    {
        private readonly IRepositoryFactory repositoryFactory;
        private readonly IMonthlyExpenseMapper monthlyExpenseMapper;
        private readonly IUserSessionService userSessionService;

        public MonthlyExpenseService(IRepositoryFactory repositoryFactory,
            IMonthlyExpenseMapper monthlyExpenseMapper,
            IUserSessionService userSessionService)
        {
            this.repositoryFactory = repositoryFactory;
            this.monthlyExpenseMapper = monthlyExpenseMapper;
            this.userSessionService = userSessionService;
        }

        public IEnumerable<MonthlyExpenseModel> GetMonthlyExpenses()
        {
            using (var monthlyExpenseRepository = repositoryFactory.CreateMonthlyExpenseRepository())
            {
                var user = userSessionService.GetAuthenticatedUser();

                var MonthlyExpenses = monthlyExpenseRepository
                    .Where(x => x.UserId.Equals(user.Id))
                    .Select(x => monthlyExpenseMapper.ToModel(x))
                    .ToList();

                return MonthlyExpenses;
            }
        }

        public OperationResult Remove(int id)
        {
            using (var monthlyExpenseRepository = repositoryFactory.CreateMonthlyExpenseRepository())
            {
                var user = userSessionService.GetAuthenticatedUser();

                var entity = monthlyExpenseRepository.GetById(id);

                if (!user.Id.Equals(entity.UserId)) return OperationResult.Error("Not allowed");

                monthlyExpenseRepository.Remove(id);

                monthlyExpenseRepository.Commit();

                return OperationResult.Succeed();
            }
        }

        public OperationResult Save(MonthlyExpenseModel model)
        {
            using (var monthlyExpenseRepository = repositoryFactory.CreateMonthlyExpenseRepository())
            {
                var user = userSessionService.GetAuthenticatedUser();

                var entity = monthlyExpenseRepository.GetById(model.Id);

                if (entity == null)
                {
                    entity = monthlyExpenseMapper.ToEntity(model);
                    entity.UserId = user.Id;

                    monthlyExpenseRepository.Add(entity);
                }
                else
                {
                    if (!user.Id.Equals(entity.Id)) return OperationResult.Error("Not allowed");

                    monthlyExpenseRepository.Update(entity);
                }

                monthlyExpenseRepository.Commit();

                return OperationResult.Succeed();
            }
        }
    }
}
