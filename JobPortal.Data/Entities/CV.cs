using System.ComponentModel.DataAnnotations;

namespace JobPortal.Data.Entities
{
    public class CV
    {
        public int Id { get; set; }

        [Display(Name = "Chứng chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập chứng chỉ của bạn")]
        [StringLength(100, ErrorMessage = "Chứng chỉ không được dài quá 100 ký tự.")]
        public string Certificate { get; set; }

        [Display(Name = "Chuyên ngành")]
        [Required(ErrorMessage = "Vui lòng nhập chuyên ngành của bạn")]
        [StringLength(100, ErrorMessage = "Chuyên ngành không được dài quá 50 ký tự.")]
        public string Major { get; set; }

        public DateTime ApplyDate { get; set; }

        [Display(Name = "Tốt nghiệp tại")]
        [Required(ErrorMessage = "Vui lòng nhập nơi bạn đã tốt nghiệp")]
        [StringLength(100, ErrorMessage = "Nơi tốt nghiệp không được dài quá 50 ký tự.")]
        public string GraduatedAt { get; set; }

        [Display(Name = "Điểm GPA")]
        [Required(ErrorMessage = "Vui lòng nhập điểm GPA của bạn")]
        [Range(0, 4, ErrorMessage = "Vui lòng nhập điểm GPA hợp lệ (0-4).")]
        public float GPA { get; set; }
            
        public Job? Job { get; set; }

        [Display(Name = "Công việc")]
        public int JobId { get; set; }

        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả của bạn")]
        public string Description { get; set; }

        [Display(Name = "Giới thiệu")]
        [Required(ErrorMessage = "Vui lòng nhập phần giới thiệu của bạn")]
        public string Introduce { get; set; }

        public AppUser? AppUser { get; set; }
        public Guid AppUserId { set; get; }

        public int Status { get; set; } // = 0 từ chối // = 1 đang chờ // = 2 chấp nhận // = 3 đã phản hồi // = -1 hủy

        [Display(Name = "Ảnh CV của bạn")]
        public string? UrlImage { get; set; } // Ảnh CV

        [Display(Name = "Số điện thoại của bạn")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại của bạn")]
        [StringLength(20, ErrorMessage = "Vui lòng nhập số điện thoại hợp lệ.", MinimumLength = 9)]
        public string Phone { get; set; }

        [Display(Name = "Email của bạn")]
        [Required(ErrorMessage = "Vui lòng nhập email của bạn")]
        [StringLength(50, ErrorMessage = "Email không được dài quá 50 ký tự.")]
        public string Email { get; set; }

        // Phản hồi từ nhà tuyển dụng
        [Display(Name = "Địa chỉ")]
        public string? EmployerAddress { get; set; }

        [Display(Name = "Số điện thoại nhà tuyển dụng")]
        [StringLength(20, ErrorMessage = "Vui lòng nhập số điện thoại hợp lệ.", MinimumLength = 9)]
        public string? EmployerPhone { get; set; }

        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        [Display(Name = "Đánh giá")]
        [Range(0, 10, ErrorMessage = "Vui lòng nhập đánh giá hợp lệ (0-10).")]
        public byte? EmployerRating { get; set; }

        public DateTime? CommentOn { get; set; }

        [Display(Name = "Thành phố")]
        [StringLength(30, ErrorMessage = "Thành phố không được dài quá 30 ký tự.")]
        public string? City { get; set; }

        [Display(Name = "Email nhà tuyển dụng")]
        [StringLength(50, ErrorMessage = "Email không được dài quá 50 ký tự.")]
        public string? EmployerEmail { get; set; }
    }
}
