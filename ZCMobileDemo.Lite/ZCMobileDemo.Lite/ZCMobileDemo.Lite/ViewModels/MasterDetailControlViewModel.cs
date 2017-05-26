﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZCMobileDemo.Lite.Interfaces;
using ZCMobileDemo.Lite.Model;
using ZCMobileDemo.Lite.Views;

namespace ZCMobileDemo.Lite.ViewModels
{
    /// <summary>
    /// MasterDetailControlViewModel class.
    /// </summary>
    public class MasterDetailControlViewModel : BaseViewModel, IZCMobileNavigation
    {
        #region Private Members
        private string header;
        private string header1;
        private string rightButton;
        private string rightButton1;
        private Page detail, detail1;
        private INavigation navigation;
        private Stack<Page> pages = new Stack<Page>();
        private Stack<Page> initialPages = new Stack<Page>();
        private bool secondContentVisibility = false;
        private bool backButtonVisibility = false;
        private int detailGridColSpan = 2;
        private int detailGridHeaderColSpan = 4;
        private const int BACK_BUTTON_PAGE_COUNT = 1;
        private const int SECOND_CONTENT_PAGE_COUNT = 1;
        private bool isExecuting = false;
        private bool hamburgerVisibility = false;
        // private bool popAsyncRequest = false;
        #endregion

        #region Public Properties
        #region Detail Container properties
        /// <summary>
        /// Property to set the application pages. e.g list and detail pages.
        /// Logic to display content as per landscape and portrait orientaions is encapsulated in this property.
        /// </summary>
        public Page Detail
        {
            get { return detail; }
            set
            {
                if (!App.IsUSerLoggedIn)
                {
                    if (detail != value || initialPages.Count == 0)
                    {
                        if (value != null)// && value.StyleId != "logintypepage" && !popAsyncRequest)
                        {
                            initialPages.Push(value);
                        }
                    }
                    detail = value;
                    RaisePropertyChanged();
                }
                else
                {
                    if (detail != value || pages.Count == 0)
                    {
                        if (Detail != null && (pages.Any() && pages.Any(x => x.StyleId == value.StyleId)))
                        {
                            pages.Pop();
                        }
                        if (value != null && value.StyleId != "dashboard")
                        {
                            pages.Push(value);
                        }
                        detail = value;
                        RaisePropertyChanged();
                    }
                }

                //This will maintain visibility of second detail page.
                GetSecondContentVisibility(App.IsUSerLoggedIn);
            }
        }

        /// <summary>
        /// Property to set the application pages. e.g list and detail pages.
        /// Logic to display content as per landscape and portrait orientaions is encapsulated in this property. 
        /// </summary>
        public Page Detail1
        {
            get { return detail1; }
            set
            {
                if (detail1 != value)
                {
                    //  if (Detail1 != null && Detail1.StyleId != value.StyleId)
                    if (Detail1 != null && (pages.Any() && pages.Any(x => x.StyleId == value.StyleId)))
                    {
                        pages.Pop();
                    }
                    if (value != null)
                    {
                        pages.Push(value);
                        GetSecondContentVisibility(App.IsUSerLoggedIn);
                    }
                    detail1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Right Button and Header Properties
        /// <summary>
        /// Header property for first content view.
        /// </summary>
        public string Header
        {
            get { return header; }
            set { header = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Header property for second content view.
        /// </summary>
        public string Header1
        {
            get { return header1; }
            set { header1 = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// ... button for first content view.
        /// </summary>
        public string RightButton
        {
            get { return rightButton; }
            set { rightButton = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// ... button for second content view.
        /// </summary>
        public string RightButton1
        {
            get { return rightButton1; }
            set { rightButton1 = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Navigation Properties
        /// <summary>
        /// This property returns current device orientation.
        /// </summary>
        public bool Isportrait
        {
            get
            {
                return (App.Current.MainPage.Height > App.Current.MainPage.Width);
            }
        }
        public IReadOnlyList<Page> ModalStack { get { return navigation.ModalStack; } }

        public IReadOnlyList<Page> NavigationStack
        {
            get
            {
                if (pages.Count == 0)
                {
                    return navigation.NavigationStack;
                }
                var implPages = navigation.NavigationStack;
                MasterDetailControl master = null;
                var beforeMaster = implPages.TakeWhile(d =>
                {
                    master = d as MasterDetailControl;
                    return master != null || d.GetType() == typeof(MasterDetailControl);
                }).ToList();
                beforeMaster.AddRange(pages);
                beforeMaster.AddRange(implPages.Where(d => !beforeMaster.Contains(d)
                    && d != master));
                return new ReadOnlyCollection<Page>(navigation.NavigationStack.ToList());
            }
        }

        /// <summary>
        /// Gets the page count of the stack.
        /// </summary>
        public int PageCount
        {
            get
            {
                return pages.Count;
            }
        }

        /// <summary>
        /// Gets the page count of the stack.
        /// </summary>
        public int InitialPageCount
        {
            get
            {
                return initialPages.Count;
            }
        }
        #endregion

        #region Visibility Control and Grid ColumnSpan Properties
        /// <summary>
        /// HamburgerVisibility property
        /// </summary>
        public bool HamburgerVisibility
        {
            get
            {
                return hamburgerVisibility;
            }
            set
            {
                hamburgerVisibility = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// SecondContentVisibility property
        /// </summary>
        public bool SecondContentVisibility
        {
            get
            {
                return secondContentVisibility;
            }
            set
            {
                secondContentVisibility = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// BackButtonVisibility property.
        /// </summary>
        public bool BackButtonVisibility
        {
            get
            {
                return backButtonVisibility;
            }
            set
            {
                backButtonVisibility = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// DetailGridColSpan property to set the page as per orientation. It defines the colspan for grid for proper display as per orientation.
        /// </summary>
        public int DetailGridColSpan
        {
            get
            {
                return detailGridColSpan;
            }
            set
            {
                detailGridColSpan = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///  DetailGridColSpan property to set the page as per orientation. It defines the colspan for grid for proper display header as per orientation.
        /// </summary>
        public int DetailGridHeaderColSpan
        {
            get
            {
                return detailGridHeaderColSpan;
            }
            set
            {
                detailGridHeaderColSpan = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// returns true if any operation is being executed. it is used to manage activity indicator
        /// </summary>
        public bool IsExecuting
        {
            get
            {
                return isExecuting;
            }
            set
            {
                isExecuting = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPageEnabled));
            }
        }

        /// <summary>
        ///  returns false if any operation is being executed. it is used to disable events when activity indicator is running.
        /// </summary>
        public bool IsPageEnabled
        {
            get
            {
                return !IsExecuting;
            }
        }

        #endregion
        #endregion

        #region Commands
        private RelayCommand _Back;

        public RelayCommand Back
        {
            get
            {
                return _Back ?? (_Back = new RelayCommand(() =>
                {
                    if (App.IsUSerLoggedIn)
                    {
                        App.MasterDetailVM.IsExecuting = true;
                        App.MasterDetailVM.PopAsync1();
                        App.MasterDetailVM.IsExecuting = false;
                    }
                    else
                    {
                        App.MasterDetailVM.PopAsyncInitialPages();
                    }
                }));
            }
            set { _Back = value; }
        }
        private RelayCommand _Hamburger;

        public RelayCommand Hamburger
        {
            get
            {
                return _Hamburger ?? (_Hamburger = new RelayCommand(() =>
                {
                    App.MasterDetailVM.IsExecuting = true;
                    App.UserSession.SideContentVisibility = (!App.UserSession.SideContentVisibility);
                    App.MasterDetailVM.RaisePropertyChanged("SideContentVisible");
                    App.MasterDetailVM.IsExecuting = false;
                }));
            }
            set { _Hamburger = value; }
        }

        #endregion

        #region Push and Pop Methods
        /// <summary>
        /// This is the method to push pages for navigation.
        /// It manages the navigation stack to push pages as per orientation and size of stack.
        /// </summary>
        /// <param name="navigationData"></param>
        public void PushAsync(ZCMobileNavigationData navigationData)
        {
            if (Isportrait || pages.Count == 0) // This is for potrait mode
            {
                Header = navigationData.NextPageTitle;
                PushAsync(navigationData.NextPage);
            }
            else//This is for landscape mode
            {
                Header = navigationData.CurrentPageTitle;
                Header1 = navigationData.NextPageTitle;
                PushAsync(navigationData.CurrentPage);
                PushAsync1(navigationData.NextPage);
            }
        }

        /// <summary>
        /// Use this method for navigation before login where there is no need dealing with second detail container.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public Task PushAsync(Page page)
        {
            Detail = page;
            return Task.FromResult(page);
        }

        //public Task PushAsyncInitialPage(Page page)
        //{
        //    Detail = page;
        //    return Task.FromResult(page);
        //}

        public Task PushAsync1(Page page)
        {
            Detail1 = page;
            return Task.FromResult(page);
        }

        public Task PushAsync(Page page, bool animated)
        {
            Detail = page;
            return Task.FromResult(page);
        }

        public Task PushAsync1(Page page, bool animated)
        {
            Detail1 = page;
            return Task.FromResult(page);
        }

        /// <summary>
        /// This method handles submit event when the submitted data is to the previous page.
        /// PopAsync is not useful as it retreives the previous page and setting updated binding context can not be set.
        /// We will make this method obsolete as message center seems the better approach.
        /// </summary>
        /// <param name="previousPage"></param>
        /// <returns></returns>
        public Task<Page> PushAsyncPreviousPage(Page previousPage = null)
        {
            Page page = null;
            Page page1 = null;
            //if (pages.Count > 0)
            // {
            if (Isportrait && pages.Count > BACK_BUTTON_PAGE_COUNT)
            {
                pages.Pop();
                pages.Pop();
                Header = App.PageTitels[previousPage.StyleId];
                PushAsync(previousPage);
                //   RaisePropertyChanged("Detail");
            }
            else if (pages.Count > BACK_BUTTON_PAGE_COUNT)
            {
                if (pages.Count == 2)
                {
                    page1 = pages.Pop();
                    page = pages.Pop();
                    Header = App.PageTitels[page.StyleId];
                    PushAsync(previousPage);
                }
                else
                {
                    pages.Pop();
                    page = pages.Pop();
                    page1 = pages.Pop();
                    Header = App.PageTitels[page1.StyleId];
                    Header1 = App.PageTitels[page.StyleId];
                    PushAsync(page1);
                    PushAsync1(previousPage);
                }
                //    RaisePropertyChanged("Detail");
            }
            else//if (pages != null && pages.Count == 1)
            {
                page = pages.Pop();
                Header = App.PageTitels[page.StyleId];
                PushAsync(Detail);
                //   RaisePropertyChanged("Detail");
                // PushAsync1(Detail);
            }
            //_detail = page;
            //  RaisePropertyChanged("Detail");
            //  }
            return page != null ? Task.FromResult(page) : Task.FromResult(previousPage);
        }

        /// <summary>
        /// This method is used to pop pages on pressing back button.
        /// It pops up the pages as per orientation and detail container visibility.
        /// </summary>
        /// <returns></returns>
        public Task<Page> PopAsync1()
        {
            Page page = null;
            Page page1 = null;
            //if (pages.Count > 0)
            // {
            if (Isportrait && pages.Count > BACK_BUTTON_PAGE_COUNT)
            {
                pages.Pop();
                page = pages.Pop();
                Header = App.PageTitels[page.StyleId];
                PushAsync(page);
            }
            else if (pages.Count > BACK_BUTTON_PAGE_COUNT)
            {
                if (pages.Count == 2)
                {
                    page1 = pages.Pop();
                    page = pages.Pop();
                    Header = App.PageTitels[page.StyleId];
                    PushAsync(page);
                }
                else
                {
                    pages.Pop();
                    page = pages.Pop();
                    page1 = pages.Pop();
                    Header = App.PageTitels[page1.StyleId];
                    Header1 = App.PageTitels[page.StyleId];
                    PushAsync(page1);
                    PushAsync1(page);
                }
            }
            else
            {
                page = pages.Pop();
                Header = App.PageTitels[page.StyleId];
                PushAsync(Detail);
            }
            return page != null ? Task.FromResult(page) : navigation.PopAsync();
        }

        /// <summary>
        /// This method pops up the pages where there is no need dealing with second container.
        /// </summary>
        /// <returns></returns>
        public Task<Page> PopAsync()
        {
            Page page = null;
            if (pages.Count > 0)
            {
                page = pages.Pop();
                detail = page;
                RaisePropertyChanged("Detail");
            }
            return page != null ? Task.FromResult(page) : navigation.PopAsync();
        }

        /// <summary>
        /// This method is for poping up pages for navigation before login.
        /// </summary>
        /// <param name="logoutRequest"></param>
        /// <returns></returns>
        public Task<Page> PopAsyncInitialPages(bool logoutRequest = false)
        {
            Page page = null;

            if (!logoutRequest && initialPages.Count > 1)
            {
                initialPages.Pop();
            }

            page = initialPages.Pop();
            Detail = page;
            return page != null ? Task.FromResult(page) : navigation.PopAsync();
        }

        public Task<Page> PopAsync(bool animated)
        {
            Page page = null;
            if (pages.Count > 0)
            {
                page = pages.Pop();
                detail = page;
                RaisePropertyChanged("Detail");
            }
            return page != null ? Task.FromResult(page) : navigation.PopAsync(animated);
        }

        public Task<Page> PopAsync1(bool animated)
        {
            Page page = null;
            if (pages.Count > 0)
            {
                page = pages.Pop();
                detail1 = page;
                RaisePropertyChanged("Detail1");
            }
            return page != null ? Task.FromResult(page) : navigation.PopAsync(animated);
        }

        public void InsertPageBefore(Page page, Page before)
        {
            if (pages.Contains(before))
            {
                var list = pages.ToList();
                var indexOfBefore = list.IndexOf(before);
                list.Insert(indexOfBefore, page);
                pages = new Stack<Page>(list);
            }
            else
            {
                navigation.InsertPageBefore(page, before);
            }
        }

        public Task<Page> PopModalAsync()
        {
            return navigation.PopModalAsync();
        }

        public Task<Page> PopModalAsync(bool animated)
        {
            return navigation.PopModalAsync(animated);
        }

        public Task PopToRootAsync()
        {
            var firstPage = navigation.NavigationStack[0];
            if (firstPage is MasterDetailControl
                || firstPage.GetType() == typeof(MasterDetailControl))
            {
                pages = new Stack<Page>(new[] { pages.FirstOrDefault() });
                return Task.FromResult(firstPage);
            }
            return navigation.PopToRootAsync();
        }

        public Task PopToRootAsync(bool animated)
        {
            var firstPage = navigation.NavigationStack[0];
            if (firstPage is MasterDetailControl
                || firstPage.GetType() == typeof(MasterDetailControl))
            {
                pages = new Stack<Page>(new[] { pages.FirstOrDefault() });
                return Task.FromResult(firstPage);
            }
            return navigation.PopToRootAsync(animated);
        }

        public Task PushModalAsync(Page page)
        {
            return navigation.PushModalAsync(page);
        }

        public Task PushModalAsync(Page page, bool animated)
        {
            return navigation.PushModalAsync(page, animated);
        }

        public void RemovePage(Page page)
        {
            if (pages.Contains(page))
            {
                var list = pages.ToList();
                list.Remove(page);
                pages = new Stack<Page>(list);
            }
            navigation.RemovePage(page);
        }

        /// <summary>
        /// Clears the page stack.
        /// </summary>
        public void RemoveAllPages()
        {
            if (pages.Count > 0)
            {
                pages.Clear();
            }
        }

        /// <summary>
        /// Clears the initial page stack.
        /// </summary>
        public void RemoveAllInitialPages()
        {
            if (initialPages.Count > 0)
            {
                initialPages.Clear();
            }
        }

        public void SetNavigation(INavigation navigation)
        {
            this.navigation = navigation;
        }
        #endregion

        #region Orientation change handling methods
        /// <summary>
        /// To do
        /// </summary>
        /// <param name="orientationChanges"></param>
        public void AdjustScreenOnOrientationChange(bool orientationChanges = false)
        {
            this.GetSecondContentVisibility(true, orientationChanges);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Manages second content visibility.
        /// </summary>
        /// <param name="isUSerLoggedIn"></param>
        /// <param name="orientationChanges"></param>
        private void GetSecondContentVisibility(bool isUSerLoggedIn, bool orientationChanges = false)
        {
            App.MasterDetailVM.SecondContentVisibility = (isUSerLoggedIn ? (!Isportrait && pages.Count > SECOND_CONTENT_PAGE_COUNT) : false);
            App.MasterDetailVM.BackButtonVisibility = (isUSerLoggedIn ? (Device.OS == TargetPlatform.iOS && pages.Count > BACK_BUTTON_PAGE_COUNT) : (Device.OS == TargetPlatform.iOS && initialPages.Count > 1));
            App.MasterDetailVM.DetailGridColSpan = (isUSerLoggedIn ? ((!Isportrait && pages.Count > SECOND_CONTENT_PAGE_COUNT) ? 1 : 2) : 2);
            App.MasterDetailVM.DetailGridHeaderColSpan = (isUSerLoggedIn ? ((!Isportrait && pages.Count > SECOND_CONTENT_PAGE_COUNT) ? 1 : 4) : 4);
        }
        #endregion
    }
}
