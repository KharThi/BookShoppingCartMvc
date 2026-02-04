namespace BookStoreApi.Models
{
	public class EmailRequest
	{
		public string To { get; set; }
		public string Subject { get; set; }
		public string Content { get; set; }
		public string[] AttachmentFilePaths { get; set; } = Array.Empty<string>();
	}
}
