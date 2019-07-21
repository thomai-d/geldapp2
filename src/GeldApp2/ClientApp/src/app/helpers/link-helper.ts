export class LinkHelper {

  /* Opens a download link by temporarily setting a auth cookie */
  public static openDownloadLink(url: string) {
    document.cookie = `X-Authorization-Bearer=${localStorage.getItem('authToken')}; Max-Age=10; Path=${url}`;
    window.open(url, '_blank');
  }

}
