using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace CoffeeManagement.BLL.Services
{
    public interface IAuthorizationService
    {
        bool IsInRole(params int[] roleIds);
        bool CanAccessControl(string controlKey);
        IEnumerable<string> GetAllowedControlsForCurrentUser();
    }

    public class AuthorizationService : IAuthorizationService
    {
        // map controlKey -> allowed roleIds
        // controlKey phải khớp với key bạn dùng ở LoadControl (ví dụ "Users","MenuUI","StaffOrders",...)
        private readonly Dictionary<string, int[]> _controlRoleMap = new Dictionary<string, int[]>
        {
            // admin (1) có quyền tất cả (we can also use null/empty to indicate all)
            // staff (2) allowed list per your requirement:
            { "Users",              new[] { 1 } },                 // only admin
            { "Menu",               new[] { 1, 2, 3 } },          // everyone
            { "MenuUI",             new[] { 1, 2, 3 } },          // MenuUI available for all per previous usage? (adjust if needed)
            { "StaffOrders",        new[] { 1, 2 } },             // staff + admin
            { "AdminAllOrdersView", new[] { 1, 2 } },             // you listed AdminAllOrdersView for RoleId=2 allowed
            { "AdminDashboard",     new[] { 1 } },                 // only admin
            { "Profile",            new[] { 1, 2, 3 } },
            { "OrderHistory",       new[] { 1, 3 } },             // admin + customer (you asked RoleId=3 see OrderHistory)
            { "Promotion",          new[] { 1, 2 } },             // AdminPromotionsView for staff+admin
            // add other control keys here...
        };

        // helper to return current user's role id
        private int? CurrentRoleId()
        {
            var u = AppSessions.CurrentUser;
            if (u == null) return null;
            return u.RoleId;
        }

        public bool IsInRole(params int[] roleIds)
        {
            var r = CurrentRoleId();
            if (!r.HasValue) return false;
            return roleIds.Contains(r.Value);
        }

        public bool CanAccessControl(string controlKey)
        {
            if (string.IsNullOrWhiteSpace(controlKey)) return false;

            // admin (1) allow everything
            if (IsInRole(1)) return true;

            if (!_controlRoleMap.TryGetValue(controlKey, out var allowed))
            {
                // if not defined, deny by default (safer)
                return false;
            }

            var r = CurrentRoleId();
            if (!r.HasValue) return false;
            return allowed.Contains(r.Value);
        }

        public IEnumerable<string> GetAllowedControlsForCurrentUser()
        {
            // admin -> return all keys
            if (IsInRole(1)) return _controlRoleMap.Keys.ToList();

            var r = CurrentRoleId();
            if (!r.HasValue) return Enumerable.Empty<string>();

            return _controlRoleMap
                    .Where(kv => kv.Value.Contains(r.Value))
                    .Select(kv => kv.Key)
                    .ToList();
        }
    }
}
