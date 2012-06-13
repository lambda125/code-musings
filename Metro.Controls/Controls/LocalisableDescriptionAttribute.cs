using System;

namespace Metro.Controls
{
    /// <summary>
    /// Attribute for localization.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class LocalisableDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="LocalisableDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="resourcesType">Type of the resources.</param>
        public LocalisableDescriptionAttribute (string description) //, Type resourcesType = null
        {
            //_resourcesType = resourcesType;
            _description = description;
        }

        private string _description;
        /// <summary>
        /// Get the string value from the resources.
        /// </summary>
        /// <value></value>
        /// <returns>The description stored in this attribute.</returns>
        public string Description
        {
            get
            {
                if (!_isLocalized)
                {
                    //TODOGlobalisation use our custom resource manager here to implement localisation

                    //ResourceManager resMan =
                    //     _resourcesType.InvokeMember(
                    //     @"ResourceManager",
                    //     BindingFlags.GetProperty | BindingFlags.Static |
                    //     BindingFlags.Public | BindingFlags.NonPublic,
                    //     null,
                    //     null,
                    //     new object[] { }) as ResourceManager;

                    //CultureInfo culture =
                    //     _resourcesType.InvokeMember(
                    //     @"Culture",
                    //     BindingFlags.GetProperty | BindingFlags.Static |
                    //     BindingFlags.Public | BindingFlags.NonPublic,
                    //     null,
                    //     null,
                    //     new object[] { }) as CultureInfo;

                    //_isLocalized = true;

                    //if (resMan != null)
                    //{
                    //    DescriptionValue =
                    //         resMan.GetString(DescriptionValue, culture);
                    //}
                }

                return _description;
            }
        }

        //private readonly Type _resourcesType;
        private bool _isLocalized;
    }
}
