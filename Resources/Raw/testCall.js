window.rep = function (input) {
    return 'Input is ' + input;
};

window.getMusicURL = async (songmid, quality = "320", origin = false) => {
    return await fetch("https://u.y.qq.com/cgi-bin/musicu.fcg", {
        headers: {
            accept: "application/json, text/plain, */*",
            "accept-language": "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6",
            "content-type": "application/json;charset=UTF-8",
            priority: "u=1, i",
            "sec-ch-ua-mobile": "?0",
            "sec-fetch-dest": "empty",
            "sec-fetch-mode": "cors",
            "sec-fetch-site": "none",
            "sec-fetch-storage-access": "active",
        },
        referrer: "https://y.qq.com/",
        body: '{"req_1":{"module":"vkey.GetVkeyServer","method":"CgiGetVkey","param":{"filename":["PREFIXSONGMIDSONGMID.SUFFIX"],"guid":"10000","songmid":["SONGMID"],"songtype":[0],"uin":"0","loginflag":1,"platform":"20"}},"loginUin":"0","comm":{"uin":"0","format":"json","ct":24,"cv":0}}'
            .replaceAll("SONGMID", songmid)
            .replaceAll(
                "PREFIX",
                quality.toLowerCase() == "m4a"
                    ? "C400"
                    : quality == "128"
                        ? "M500"
                        : "M800"
            )
            .replaceAll("SUFFIX", quality.toLowerCase() == "m4a" ? "m4a" : "mp3"),
        method: "POST",
        mode: "cors",
        credentials: "include",
    })
        .then((res) => res.json())
        .then((data) => {
            if (origin) return data;
            else return data.req_1.data.sip[0] + data.req_1.data.midurlinfo[0].purl;
        })
        .catch((err) => {
            console.log(err);
        });
};

window.getSongList = async (categoryID, origin = false) => {
    return await fetch(
        "https://i.y.qq.com/qzone-music/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&nosign=1&disstid=CATEGORYID&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0".replaceAll(
            "CATEGORYID",
            categoryID
        )
    )
        .then((res) => res.json())
        .then((data) => {
            if (origin) return data;
            else return data.cdlist[0].songlist;
        })
        .catch((err) => {
            console.log(err);
        });
};

window.getSongListName = async (categoryID, origin = false) => {
    return await getSongList(categoryID, true).then((data) => {
        if (origin) return data;
        else return data.cdlist[0].dissname;
    });
};

window.searchWithKeyword = async (
    keyword,
    searchType = 0,
    resultNum = 50,
    pageNum = 1,
    origin = false
) => {
    return await fetch("https://u.y.qq.com/cgi-bin/musicu.fcg", {
        credentials: "include",
        headers: {
            "User-Agent":
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0",
            Accept: "application/json, text/plain, */*",
            "Accept-Language":
                "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2",
            "Content-Type": "application/json;charset=utf-8",
            "Sec-Fetch-Dest": "empty",
            "Sec-Fetch-Mode": "cors",
            "Sec-Fetch-Site": "same-origin",
        },
        body: '{"comm":{"ct":"19","cv":"1859","uin":"0"},"req":{"method":"DoSearchForQQMusicDesktop","module":"music.search.SearchCgiService","param":{"grp":1,"num_per_page":RESULTNUM,"page_num":PAGENUM,"query":"KEYWORD","search_type":SEARCHTYPE}}}'
            .replaceAll("KEYWORD", keyword)
            .replaceAll("RESULTNUM", resultNum)
            .replaceAll("PAGENUM", pageNum)
            .replaceAll("SEARCHTYPE", searchType),
        method: "POST",
        mode: "cors",
    })
        .then((res) => res.json())
        .then((data) => {
            if (origin) return data;
            else {
                switch (searchType) {
                    case 0:
                    case 7:
                        return data.req.data.body.song;
                    case 2:
                        return data.req.data.body.album;
                    case 3:
                        return data.req.data.body.songlist;
                    case 4:
                        return data.req.data.body.mv;
                    case 8:
                        return data.req.data.body.user;
                    default:
                        return data.req.data.body;
                }
            }
        })
        .catch((err) => {
            console.log(err);
        });
};

/*
window.parseLyric = (data) => {
    let parsed = {
        ti: "",
        ar: "",
        al: "",
        by: "",
        offset: "",
        count: 0,
        haveTrans: false,
        lyric: [],
    };
    let lyric = data.lyric.split("\n");
    let trans = data.trans.split("\n");
    parsed.haveTrans = !(trans == "");
    let substr = (str) => str.substring(str.indexOf(":") + 1, str.indexOf("]"));
    if (!lyric[0].startsWith("[0")) {
        parsed.ti = substr(lyric[0]);
        parsed.ar = substr(lyric[1]);
        parsed.al = substr(lyric[2]);
        parsed.by = substr(lyric[3]);
        parsed.offset = substr(lyric[4]);
        lyric = lyric.slice(5);
        if (parsed.haveTrans) {
            trans = trans.slice(5);
        }
    }
    parsed.count = lyric.length;
    for (let i = 0; i < parsed.count; i++) {
        let ele = { time: "", lyric: "", trans: "" };
        ele.time = lyric[i].substring(1, lyric[i].indexOf("]"));
        ele.lyric = lyric[i].substring(lyric[i].indexOf("]") + 1);
        if (parsed.haveTrans) {
            ele.trans = trans[i].substring(trans[i].indexOf("]") + 1);
        }
        parsed.lyric.push(ele);
    }

    return parsed;
};
*/
/*
window.getSongLyric = async (songmid, parse = false, origin = false) => {
    return await fetch(
        "https://i.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid=SONGMID&g_tk=5381&format=json&inCharset=utf8&outCharset=utf-8&nobase64=1".replaceAll(
            "SONGMID",
            songmid
        )
    )
        .then((res) => res.json())
        .then((data) => {
            if (origin) return data;
            else {
                if (!parse) {
                    return data.lyric + "\n" + data.trans;
                } else return parseLyric(data);
            }
        })
        .catch((err) => {
            console.log(err);
        });
};
*/
window.getAlbumSongList = async (albummid, origin = false) => {
    return await fetch(
        "https://i.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?platform=h5page&albummid=ALBUMMID&g_tk=938407465&uin=0&format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5&needNewCode=1&_=1459961045571".replaceAll(
            "ALBUMMID",
            albummid
        )
    )
        .then((res) => res.json())
        .then((data) => {
            if (origin) return data;
            else return data.data.list;
        })
        .catch((err) => {
            console.log(err);
        });
};

window.getAlbumName = async (albummid, origin = false) => {
    return await getAlbumSongList(albummid, true).then((data) => {
        if (origin) return data;
        else return data.data.name;
    });
};

window.getMVInfo = async (vid, origin = true) => {
    return await fetch("https://u.y.qq.com/cgi-bin/musicu.fcg", {
        credentials: "include",
        headers: {
            "User-Agent":
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0",
            Accept: "*/*",
            "Accept-Language":
                "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2",
            "Content-type": "application/x-www-form-urlencoded",
            "Sec-Fetch-Dest": "empty",
            "Sec-Fetch-Mode": "cors",
            "Sec-Fetch-Site": "same-site",
        },
        referrer: "https://y.qq.com/",
        body: '{"comm":{"ct":6,"cv":0,"g_tk":1646675364,"uin":0,"format":"json","platform":"yqq"},"mvInfo":{"module":"music.video.VideoData","method":"get_video_info_batch","param":{"vidlist":["VID"],"required":["vid","type","sid","cover_pic","duration","singers","new_switch_str","video_pay","hint","code","msg","name","desc","playcnt","pubdate","isfav","fileid","filesize_v2","switch_pay_type","pay","pay_info","uploader_headurl","uploader_nick","uploader_uin","uploader_encuin","play_forbid_reason"]}},"mvUrl":{"module":"music.stream.MvUrlProxy","method":"GetMvUrls","param":{"vids":["VID"],"request_type":10003,"addrtype":3,"format":264,"maxFiletype":60}}}'.replaceAll(
            "VID",
            vid
        ),
        method: "POST",
        mode: "cors",
    })
        .then((res) => res.json())
        .then((data) => data)
        .catch((err) => {
            console.log(err);
        });
};

window.getSingerInfo = async (singermid, origin = false) => {
    return await fetch(
        "https://u.y.qq.com/cgi-bin/musicu.fcg?format=json&loginUin=0&hostUin=0inCharset=utf8&outCharset=utf-8&platform=yqq.json&needNewCode=0&data=%7B%22comm%22%3A%7B%22ct%22%3A24%2C%22cv%22%3A0%7D%2C%22singer%22%3A%7B%22method%22%3A%22get_singer_detail_info%22%2C%22param%22%3A%7B%22sort%22%3A5%2C%22singermid%22%3A%22SINGERMID%22%2C%22sin%22%3A0%2C%22num%22%3A50%7D%2C%22module%22%3A%22music.web_singer_info_svr%22%7D%7D".replaceAll(
            "SINGERMID",
            singermid
        )
    )
        .then((res) => res.json())
        .then((data) => (origin ? data : data.singer.data))
        .catch((err) => {
            console.log(err);
        });
};

window.getAlbumCoverImage = (albummid) => {
    return "https://y.gtimg.cn/music/photo_new/T002R300x300M000ALBUMMID.jpg".replaceAll(
        "ALBUMMID",
        albummid
    );
};